
internal class Program
{
    private static void Main(string[] args)
    {
        using var game = new NeoAnimLib.MonogameDemo.Core();
        game.Run();
    }
}