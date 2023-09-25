using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NeoAnimLib.Nodes;

namespace NeoAnimLib.MonogameDemo;

public class Core : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private FontSystem font;
    private Texture2D pixel;

    private MixAnimNode rootNode;
    private bool flipFlop;
    private KeyboardState oldKState;

    public Core()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        graphics.PreferredBackBufferWidth *= 2;
        graphics.PreferredBackBufferHeight *= 2;
        graphics.ApplyChanges();

        Window.Title = "NeoAnimLib Demo";

        InitNodes();
    }

    private void InitNodes()
    {
        rootNode = new MixAnimNode("Root Node");

        rootNode.Add(new ClipAnimNode(Animations.VerticalHover));
        //rootNode.Add(new ClipAnimNode(Animations.HorizontalHover));
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        LoadFont();

        pixel = new Texture2D(graphics.GraphicsDevice, 1, 1);
        pixel.SetData(new[] {Color.White});
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        pixel.Dispose();
        spriteBatch.Dispose();
        font.Dispose();
        graphics.Dispose();
    }

    private void LoadFont()
    {
        font = new FontSystem();
        font.AddFont(File.ReadAllBytes("./Content/OpenSans-Bold.ttf"));
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var kState = Keyboard.GetState();
        if (kState.IsKeyDown(Keys.Space) && oldKState.IsKeyUp(Keys.Space))
        {
            var first  = new ClipAnimNode(Animations.HorizontalHover) { TargetDuration = 3f };
            var second = new ClipAnimNode(Animations.VerticalHover);

            first.ContinueWith(second, 1f);

            rootNode.Clear();
            rootNode.Add(first);
        }

        oldKState = kState;

        rootNode.LocalSpeed = 1f;

        rootNode.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        var f = font.GetFont(50);

        spriteBatch.Begin();

        spriteBatch.DrawString(f, rootNode.PrintDebugTree(), new Vector2(10, 10), Color.White);

        DrawAnimation();

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawAnimation()
    {
        using var sample = rootNode.Sample(new SamplerInput 
        { 
            MissingPropertyBehaviour = MissingPropertyBehaviour.UseDefaultValue,
            DefaultValueSource = Animations.DefaultValueSource
        });

        if (sample == null)
            return;

        float x = sample.GetPropertyValue("X", Animations.DefaultValueSource);
        float y = sample.GetPropertyValue("Y", Animations.DefaultValueSource);

        spriteBatch.Draw(pixel, new Rectangle((int)x, (int)y, 100, 100), Color.Magenta);
    }
}