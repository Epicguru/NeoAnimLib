using System;
using System.Diagnostics;

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
        /// Upon reaching this target loop count, it is an end condition and the behaviour described in <see cref="EndBehaviour"/> is triggered
        /// and the <see cref="OnEndPlay"/> event is raised.
        /// See also: <seealso cref="LoopCount"/>.
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
        /// The target duration, in seconds, that this clip will play for.
        /// Upon reaching this duration, it is an end condition and the behaviour described in <see cref="EndBehaviour"/> is triggered
        /// and the <see cref="OnEndPlay"/> event is raised.
        /// Must be greater than 0 or null.
        /// This duration is in <b>scaled</b> time, meaning that it is affected by <see cref="AnimNode.Speed"/>.
        /// </summary>
        public float? TargetDuration
        {
            get => targetDuration;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == targetDuration)
                    return;

                if (value <= 0f)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Target duration must be greater than 0, or null. Input: {value}.");

                targetDuration = value;
            }
        }

        /// <summary>
        /// The time, in seconds, for which this clip has been playing.
        /// The value is scaled by <see cref="AnimNode.Speed"/>.
        /// </summary>
        public float Duration { get; protected set; }

        /// <summary>
        /// The time, in seconds, for which this clip has been playing.
        /// This value is unaffected by <see cref="AnimNode.Speed"/>.
        /// </summary>
        public float DurationUnscaled { get; protected set; }

        /// <summary>
        /// Returns true if this clip has ever been stepped (see <see cref="AnimNode.Step(float)"/>.
        /// See also <see cref="AnimNode.IsEnded"/>.
        /// Note: the clip may have 'ended' without starting if an end condition was met before the very first
        /// first attempt to step it.
        /// </summary>
        public bool HasStartedPlaying { get; private set; }

        /// <summary>
        /// Changes what happens when an end condition, such as <see cref="TargetLoopCount"/>, is met.
        /// </summary>
        public ClipEndBehaviour EndBehaviour { get; set; } = ClipEndBehaviour.RemoveFromParent;

        /// <summary>
        /// Invoked once on the first frame that this clip starts playing,
        /// even if it's <see cref="AnimNode.Speed"/> or <see cref="AnimNode.Weight"/> are 0.
        /// </summary>
        public event Action<ClipAnimNode>? OnStartPlay;

        /// <summary>
        /// Invoked once per frame while this clip is playing,
        /// even if it's <see cref="AnimNode.Speed"/> or <see cref="AnimNode.Weight"/> are 0.
        /// </summary>
        public event Action<ClipAnimNode>? OnPlaying;

        /// <summary>
        /// Invoked when this animation clip has completed a loop.
        /// This event will never be raised if the <see cref="Clip"/> has a length of 0.
        /// Note: this is raised whenever the clip passes
        /// </summary>
        public event Action<ClipAnimNode>? OnLoop;

        /// <summary>
        /// Invoked once when this animation clip reaches it's end condition.
        /// After this event is raised, the behaviour defined by <see cref="EndBehaviour"/> is invoked.
        /// This will not be raised more than once regardless of <see cref="EndBehaviour"/>.
        /// Note: this event is only raised if OnStartPlay has been raised, meaning that if the end condition is met
        /// before the clip has had a chance to step, then this will not be raised.
        /// </summary>
        public event Action<ClipAnimNode>? OnEndPlay;

        /// <summary>
        /// Used to track which loop we were on last local step.
        /// Use for testing and debugging purposes only.
        /// </summary>
        protected int LastLoopIndex;

        private bool hasDoneEndBehaviour;
        private bool hasRaisedEndEvent;
        private bool isStopRequested;
        private int? targetLoopCount;
        private float? targetDuration;

        /// <summary>
        /// Creates a new <see cref="ClipAnimNode"/> with a specified <see cref="IAnimClip"/>.
        /// The clip can not be null and it can not be changed after this point.
        /// </summary>
        public ClipAnimNode(IAnimClip clip)
            : base(clip?.Name ?? "")
        {
            Clip = clip ?? throw new ArgumentNullException(nameof(clip));
        }

        /// <inheritdoc/>
        public override AnimSample Sample(in SamplerInput input)
        {
            var sample = AnimSample.Create(LocalTime);
            Clip.Sample(sample, LocalTime);
            return sample;
        }

        /// <inheritdoc/>
        protected override void LocalStep(float deltaTime)
        {
            // TODO sample events, loop the time etc.

            // Check end conditions before even stepping.
            if (CheckEndConditions())
            {
                IsEnded = true;
                TryRaiseEndEvent();
                TryPerformEndBehaviour();
                return;
            }

            // Raise playback events:
            if (!HasStartedPlaying)
            {
                HasStartedPlaying = true;
                OnStartPlay?.Invoke(this);
            }
            OnPlaying?.Invoke(this);

            // Update durations.
            Duration += MathF.Abs(deltaTime * Speed);
            DurationUnscaled += MathF.Abs(deltaTime);

            // Scale by speed from now on.
            deltaTime *= Speed;

            // Raise anim events:
            bool raiseEvents = deltaTime != 0 && !Clip.IsPose();
            if (raiseEvents)
            {
                float startTime = MathF.Min(LocalTime, LocalTime + deltaTime);
                float endTime = MathF.Max(LocalTime, LocalTime + deltaTime);
                RaiseAnimEvents(startTime, endTime);
            }

            bool noMovement = deltaTime == 0;
            if (Clip.IsPose() && !noMovement)
            {
                // For pose clips, just advance the time.
                // No loop events need raising.
                LocalTime += deltaTime;
            }
            else if (!noMovement)
            {
                // Because of the way looping works, limit the time step to the length of the clip and repeat as many times as necessary:
                float toDo = deltaTime;
                float expected = LocalTime + deltaTime;
                while (deltaTime > 0 ? toDo > 0 : toDo < 0)
                {
                    float toStep = MathF.Min(MathF.Abs(toDo), Clip.Length);
                    if (toDo < 0)
                        toStep = -toStep;

                    toDo -= toStep;
                    LocalTime += toStep;

                    // Update looping.
                    int loopIndex = (int)(LocalTime / Clip.Length);
                    if (loopIndex != LastLoopIndex)
                    {
                        LastLoopIndex = loopIndex;
                        LoopCount++;
                        OnLoop?.Invoke(this);
                    }
                }

                Debug.Assert(Math.Abs(LocalTime - expected) < 0.0001f, $"Expected LocalTime to be {expected}, got {LocalTime}");
            }

            // Check end conditions again.
            if (CheckEndConditions())
            {
                IsEnded = true;
                TryRaiseEndEvent();
                TryPerformEndBehaviour();
            }
        }

        /// <summary>
        /// Immediately sets the <see cref="AnimNode.LocalTime"/> to the specified value.
        /// <b>Important:</b> this does not raise any events, change the <see cref="LoopCount"/> or <see cref="Duration"/> or trigger any end conditions.
        /// </summary>
        public void SetLocalTime(float time)
        {
            LocalTime = time;
        }

        /// <summary>
        /// Raises all animation events between <paramref name="startTime"/> (inclusive) and <paramref name="endTime"/> (exclusive).
        /// </summary>
        protected void RaiseAnimEvents(float startTime, float endTime)
        {
            foreach (var e in Clip.GetEventsInRange(startTime, endTime))
            {
                if (e.Action == null)
                    throw new Exception($"An event was hit that has a null action! (at time {e.Time})");

                e.Action.Invoke(this);
            }
        }

        /// <summary>
        /// Immediately stops this animation clip,
        /// and triggers the behaviour defined by <see cref="EndBehaviour"/>
        /// (which is removing this clip from its parent by default).
        /// </summary>
        public virtual void Stop()
        {
            isStopRequested = true;
            TryRaiseEndEvent();
            TryPerformEndBehaviour();
            IsEnded = true;
        }

        /// <summary>
        /// Returns true if any end condition has been met.
        /// </summary>
        protected virtual bool CheckEndConditions()
        {
            // Manual stop requested:
            if (isStopRequested)
                return true;

            // Target loops:
            if (LoopCount >= TargetLoopCount)
                return true;

            // Target duration:
            if (Duration >= TargetDuration)
                return true;

            return false;
        }

        /// <summary>
        /// Attempts to raise the <see cref="OnEndPlay"/> event.
        /// It will not be raised if it has already been raised before or this clip has not been stepped.
        /// </summary>
        protected void TryRaiseEndEvent()
        {
            if (hasRaisedEndEvent || !HasStartedPlaying)
                return;

            OnEndPlay?.Invoke(this);
            hasRaisedEndEvent = true;
        }

        /// <summary>
        /// Attempts to perform the behaviour defined in <see cref="EndBehaviour"/>.
        /// It will not perform the behaviour twice even if called multiple times or <see cref="EndBehaviour"/> is changed.
        /// </summary>
        protected void TryPerformEndBehaviour()
        {
            if (hasDoneEndBehaviour)
                return;

            hasDoneEndBehaviour = true;
            switch (EndBehaviour)
            {
                case ClipEndBehaviour.RemoveFromParent:
                    Parent?.Remove(this);
                    break;

                case ClipEndBehaviour.Nothing:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(EndBehaviour), EndBehaviour.ToString());
            }
        }

        /// <inheritdoc/>
        public override string ToString() => Clip.Name ?? "ClipAnimNode (no-name clip)";
    }
}
