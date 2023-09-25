using NeoAnimLib.Clips;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// A subclass of <see cref="MixAnimNode"/> that specializes in being a transition between exactly two child <see cref="AnimNode"/>.
    /// Its main properties:
    /// <list type="number">
    /// <item>It always has exactly two child nodes, <see cref="FromNode"/> and <see cref="ToNode"/>. Neither can ever be null.</item>
    /// <item>Whenever <see cref="Remove(AnimNode)"/> is called, the specified node is replaced with a <see cref="SnapshotAnimClip"/> that captures the state of the node being removed.</item>
    /// <item><see cref="Blend"/> must be used to control the lerp between the <see cref="FromNode"/> and <see cref="ToNode"/>.
    /// It can be edited manually or <see cref="TransitionDuration"/> can be used
    /// to automatically advance <see cref="Blend"/> when <see cref="AnimNode.Step(float)"/> is called.</item>
    /// </list>
    /// </summary>
    public class TransitionNode : MixAnimNode, IHasEndEvent, IHasTimeToEnd
    {
        /// <summary>
        /// Always false on <see cref="TransitionNode"/> - it cannot be assigned to either.
        /// </summary>
        public override bool NormalizeWeights
        {
            get => false;
            set => throw new InvalidOperationException($"{nameof(NormalizeWeights)} cannot be used on a {nameof(TransitionNode)}");
        }

        /// <summary>
        /// The <see cref="AnimNode"/> that is being transitioned away from.
        /// </summary>
        public AnimNode FromNode
        {
            get => Children[0];
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Parent == null);
                value.Parent = this;
                Children[0] = value;
            }
        }

        /// <summary>
        /// The <see cref="AnimNode"/> that is being transitioned towards.
        /// </summary>
        public AnimNode ToNode
        {
            get => Children[1];
            set
            {
                Debug.Assert(value != null);
                Debug.Assert(value.Parent == null);
                value.Parent = this;
                Children[1] = value;
            }
        }

        /// <summary>
        /// The percentage along which this transition has gone from
        /// <see cref="FromNode"/> towards <see cref="ToNode"/>.
        /// Will be automatically incremented when <see cref="AnimNode.Step(float)"/> is called
        /// <b>iff</b> <see cref="TransitionDuration"/> is not null.
        /// Increasing this to 1 or more is considered an end condition, which will cause the end behaviour to be triggered the next time Step is called.
        /// </summary>
        public float Blend { get; set; }

        /// <summary>
        /// The transition duration, in seconds (affected by <see cref="AnimNode.Speed"/>),
        /// that this transition will last for. i.e. it will take <see cref="TransitionDuration"/> seconds to increment
        /// <see cref="Blend"/> from 0 to 1.
        /// The assigned value cannot be less than 0.
        /// </summary>
        public float? TransitionDuration
        {
            get => transitionDuration;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Can not set transition duration to less than 0.");
                transitionDuration = value;
            }
        }

        /// <summary>
        /// The behaviour which occurs when the end of the transition it met.
        /// Note: manually changing <see cref="Blend"/> will not trigger the end event,
        /// only <see cref="TransitionDuration"/> can be set to trigger the end.
        /// </summary>
        public TransitionEndBehaviour EndBehaviour { get; set; } = TransitionEndBehaviour.ReplaceInParent;

        /// <summary>
        /// The event raised when the end of the transition is reached, at which point the behaviour described in
        /// <see cref="EndBehaviour"/> is triggered after this event is raised.
        /// Note: manually changing <see cref="Blend"/> will not trigger the end event,
        /// only <see cref="TransitionDuration"/> can be set to trigger the end.
        /// </summary>
        public event Action<TransitionNode>? OnTransitionEnd;

        private float? transitionDuration;

        /// <summary>
        /// Creates a new transition node based on two clips.
        /// </summary>
        public TransitionNode(IAnimClip fromClip, IAnimClip toClip) : this()
        {
            // Make new clips:
            FromNode = new ClipAnimNode(fromClip);
            ToNode   = new ClipAnimNode(toClip);
        }

        /// <summary>
        /// Creates a new transition node based on two nodes.
        /// </summary>
        public TransitionNode(AnimNode fromNode, AnimNode toNode) : this()
        {
            FromNode = fromNode;
            ToNode = toNode;
        }

        /// <summary>
        /// Creates an empty transition node.
        /// </summary>
        public TransitionNode()
        {
            Children.Add(null!);
            Children.Add(null!);
        }

        /// <summary>
        /// Not supported on <see cref="TransitionNode"/>: it will throw an exception.
        /// Use <see cref="Replace(AnimNode, AnimNode?)"/> instead.
        /// </summary>
        public sealed override void Add(AnimNode node)
        {
            throw new NotImplementedException("Cannot add a clip to a TransitionNode");
        }

        /// <summary>
        /// Removes the specified child node from this transition and immediately replaces it with a <see cref="ClipAnimNode"/> that has a
        /// <see cref="SnapshotAnimClip"/> based on the node that is being removed.
        /// </summary>
        public sealed override void Remove(AnimNode node)
        {
            if (!DirectChildren.Contains(node ?? throw new ArgumentNullException(nameof(node))))
                throw new InvalidOperationException($"Node '{node}' is not a direct child of this {nameof(TransitionNode)}, so it cannot be removed.");

            using var lastSample = node.Sample(new SamplerInput
            {
                MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue
            });

            var snapshotClip = new SnapshotAnimClip(lastSample, $"Snapshot of {node}");

            var replacementNode = new ClipAnimNode(snapshotClip);
            replacementNode.SetLocalTime(node.LocalTime);
            replacementNode.LocalWeight = node.LocalWeight;
            // Copy speed even though it's a snapshot because it can be copied to other nodes later.
            replacementNode.LocalSpeed = node.LocalSpeed;

            Replace(node, replacementNode);
        }

        /// <summary>
        /// Not supported on <see cref="TransitionNode"/>: it will throw an exception.
        /// Use <see cref="Replace(AnimNode, AnimNode?)"/> or <see cref="Remove(AnimNode)"/> instead.
        /// </summary>
        public sealed override void Insert(int index, AnimNode node)
        {
            throw new NotImplementedException("Cannot insert a clip to a TransitionNode. Use Remove or Replace instead.");
        }

        /// <inheritdoc/>
        public sealed override void Replace(AnimNode existing, AnimNode? replacement)
        {
            if (!DirectChildren.Contains(existing ?? throw new ArgumentNullException(nameof(existing))))
                throw new InvalidOperationException($"Node '{existing}' is not a direct child of this {nameof(TransitionNode)}, so it cannot be replaced.");

            Debug.Assert(existing.Parent == this);
            existing.Parent = null;

            Children[Children.IndexOf(existing)] = replacement ?? throw new ArgumentNullException(nameof(replacement));
            replacement.Parent = this;
        }

        /// <inheritdoc/>
        public override AnimSample? Sample(in SamplerInput input)
        {
            EnsureChildrenNotNull();

            FromNode.LocalWeight = 1f - Blend;
            ToNode.LocalWeight = Blend;

            return base.Sample(input);
        }

        /// <inheritdoc/>
        protected override void LocalStep(float deltaTime)
        {
            EnsureChildrenNotNull();

            UpdateTransitioning(deltaTime);

            if (!IsEnded && CheckEndConditions())
            {
                OnTransitionEnd?.Invoke(this);
                TryPerformEndBehaviour();
                IsEnded = true;
            }
        }

        private void EnsureChildrenNotNull()
        {
            if (FromNode == null || ToNode == null)
                throw new InvalidOperationException("Cannot Step or Sample if there are not exactly 2 child nodes.");
        }

        private bool CheckEndConditions()
        {
            // Blend:
            if (Blend >= 1f)
                return true;

            return false;
        }

        /// <summary>
        /// Immediately stops this transition node, raises the <see cref="OnTransitionEnd"/> event and triggers the behaviour described in
        /// <see cref="EndBehaviour"/>.
        /// Does nothing if the transition is already ended (<see cref="AnimNode.IsEnded"/>).
        /// </summary>
        public void Stop()
        {
            if (IsEnded)
                return;

            OnTransitionEnd?.Invoke(this);
            TryPerformEndBehaviour();
            IsEnded = true;
        }

        /// <summary>
        /// Performs the end behaviour described by <see cref="EndBehaviour"/>.
        /// </summary>
        protected void TryPerformEndBehaviour()
        {
            switch (EndBehaviour)
            {
                case TransitionEndBehaviour.ReplaceInParent:
                    if (Parent == null)
                        return;

                    bool replaceWithSnapshot = Blend < 1f;

                    if (replaceWithSnapshot)
                    {
                        using var sample = Sample(new SamplerInput
                        {
                            MissingPropertyBehaviour = MissingPropertyBehaviour.UseKnownValue
                        });

                        var snapshot = new SnapshotAnimClip(sample, $"Snapshot of {this}");
                        var node = new ClipAnimNode(snapshot);
                        Parent.Replace(this, node);
                    }
                    else
                    {
                        var to = ToNode;
                        to.Parent = null;
                        Parent.Replace(this, to);
                    }
                    break;

                case TransitionEndBehaviour.RemoveFromParent:
                    Parent?.Remove(this);
                    break;

                case TransitionEndBehaviour.Nothing:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(EndBehaviour), EndBehaviour.ToString());
            }
        }


        /// <summary>
        /// Increments <see cref="Blend"/> based on the values of
        /// <see cref="TransitionDuration"/>.
        /// If both durations are null, then this method does nothing.
        /// <see cref="Blend"/> will not be incremented over 1 by this method.
        /// </summary>
        protected void UpdateTransitioning(float deltaTime)
        {
            if (TransitionDuration != null)
            {
                bool instant = TransitionDuration.Value == 0;
                if (instant)
                {
                    if (deltaTime * Speed != 0 && Blend < 1)
                        Blend = 1;
                }
                else
                {
                    if (Blend < 1)
                        Blend += (1f / TransitionDuration.Value) * deltaTime * MathF.Abs(Speed);
                    if (Blend > 1)
                        Blend = 1;
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterEndEvent(Action<AnimNode> endEvent)
        {
            OnTransitionEnd += endEvent ?? throw new ArgumentNullException(nameof(endEvent));
        }

        /// <inheritdoc/>
        public void UnRegisterEndEvent(Action<AnimNode> endEvent)
        {
            OnTransitionEnd -= endEvent ?? throw new ArgumentNullException(nameof(endEvent));
        }

        /// <inheritdoc/>
        public float? GetTimeToEnd()
        {
            float? doneTime = Blend * TransitionDuration;
            float? tte = TransitionDuration - doneTime;
            if (tte == null)
                return null;

            return MathF.Max(tte.Value, 0f);
        }

        /// <inheritdoc/>
        public override string ToString() => $"Transition from {FromNode} to {ToNode} ({Blend:P0})";
    }
}
