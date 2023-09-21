using System;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// An anim node that represents an animation clip.
    /// </summary>
    public class ClipAnimNode : LeafAnimNode
    {
        /// <summary>
        /// The length of the animation clip: shorthand for <see cref="Clip"/>.Length;
        /// </summary>
        public float Length => Clip.Length;

        /// <summary>
        /// The animation clip for this node.
        /// </summary>
        public readonly IAnimClip Clip;

        /// <summary>
        /// The target number of loops that will be done of the <see cref="Clip"/>.
        /// If null, it will loop indefinitely unless another end condition is met.
        /// Input must be null or greater than 0.
        /// If <see cref="Clip"/> has a duration of 0, then this property is invalid and must be left as null.
        /// </summary>
        public int? TargetLoopCount
        {
            get => targetLoopCount;
            set
            {
                if (value == targetLoopCount)
                    return;

                if (value != null)
                {
                    if (Clip.IsPose())
                        throw new InvalidOperationException($"Cannot set {nameof(TargetLoopCount)} when {nameof(Clip)} has a length of 0.");
                    if (value.Value < 1)
                        throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(TargetLoopCount)} must be at least 1. Set to null to loop indefinitely. Input was {value}.");
                }

                targetLoopCount = value;
            }
        }

        /// <summary>
        /// The number of complete loops that this clip has done.
        /// This will always be 0 if the <see cref="Clip"/> has a length of 0.
        /// </summary>
        public int LoopCount { get; protected set; }

        /// <summary>
        /// Invoked once on the first frame that this clip starts playing,
        /// even if it's <see cref="AnimNode.Speed"/> or <see cref="AnimNode.Weight"/> are 0.
        /// </summary>
        public event Action<ClipAnimNode> OnStartPlay;

        /// <summary>
        /// Invoked once per frame while this clip is playing,
        /// even if it's <see cref="AnimNode.Speed"/> or <see cref="AnimNode.Weight"/> are 0.
        /// </summary>
        public event Action<ClipAnimNode> OnPlaying;

        /// <summary>
        /// Invoked when this animation clip has completed a loop.
        /// This event will never be raised if the <see cref="Clip"/> has a length of 0.
        /// Note: this is raised whenever the clip passes
        /// </summary>
        public event Action<ClipAnimNode> OnLoop;

        private bool hasStepped;
        private int? targetLoopCount = null;
        private int lastLoopIndex;

        public ClipAnimNode(IAnimClip clip)
            : base(clip.Name ?? throw new ArgumentNullException(nameof(clip)))
        {
            Clip = clip;
        }

        /// <inheritdoc/>
        public override AnimSample Sample(in SamplerInput input)
        {
            // TODO sample the clip here.
            var sample = AnimSample.Create(LocalTime);
            Clip.Sample(sample, LocalTime);
            return sample;
        }

        protected override void LocalStep(float deltaTime)
        {
            // TODO sample events, loop the time etc.
            LocalTime += deltaTime;

            if (!Clip.IsPose())
            {
                // Update looping.
                int loopIndex = (int)(LocalTime / Clip.Length);
                if (loopIndex != lastLoopIndex)
                {
                    lastLoopIndex = loopIndex;
                    LoopCount++;
                }
            }

            float clipTime = LocalTime % Clip.Length;

            // Raise playback events:
            if (!hasStepped)
            {
                OnStartPlay?.Invoke(this);
                hasStepped = true;
            }
            OnPlaying?.Invoke(this);
        }
    }
}
