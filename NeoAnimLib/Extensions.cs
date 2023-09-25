using System;
using NeoAnimLib.Nodes;

namespace NeoAnimLib
{
    /// <summary>
    /// A collection of extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns true if this <see cref="IAnimClip"/> has a <see cref="IAnimClip.Length"/> of 0.
        /// </summary>
        public static bool IsPose(this IAnimClip clip) => clip.Length == 0f;

        /// <summary>
        /// Whenever the end condition on this node is met, it is immediately replaced with <paramref name="other"/>.
        /// </summary>
        public static T ContinueWith<T>(this T node, AnimNode other) where T : AnimNode, IHasEndEvent
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            node.RegisterEndEvent(n =>
            {
                // If parent is already null, not much we can do.
                // Throwing an exception feels a bit too disruptive.
                if (n.Parent == null)
                    return;

                n.Parent.Replace(n, other);
            });

            return node;
        }

        /// <summary>
        /// Whenever this node is approaching a known end condition (such as one set by <see cref="ClipAnimNode.TargetDuration"/> or
        /// <see cref="TransitionNode.TransitionDuration"/>) then it will begin transitioning to <paramref name="other"/>.
        /// If this node unexpectedly exits, such as by a call to <see cref="ClipAnimNode.Stop"/>, then the transition will be done immediately if
        /// <paramref name="doInstantTransitionIfEarlyExit"/> is true.
        /// </summary>
        public static T ContinueWith<T>(this T node, AnimNode other, float transitionDuration, bool doInstantTransitionIfEarlyExit = true) where T : AnimNode, IHasEndEvent, IHasTimeToEnd
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // Use other overload for instant transitions.
            if (transitionDuration <= 0f)
                return node.ContinueWith(other);

            bool hasStartedTransition = false;

            node.PostStep += (_, deltaTime) =>
            {
                if (hasStartedTransition)
                    return;

                float? tte = node.GetTimeToEnd();
                if (tte == null)
                    return;

                if (tte <= transitionDuration)
                {
                    // Start transition...
                    hasStartedTransition = true;

                    // Actual transition time should be the transition duration,
                    // or the reported time-to-end, whichever is smaller.
                    // This handles the situation where a clip is going to last less time than the requested transition time.
                    float finalTransitionDuration = MathF.Min(tte.Value, transitionDuration);
                    node.TransitionTo(other, finalTransitionDuration);
                }
            };

            if (doInstantTransitionIfEarlyExit)
            {
                node.RegisterEndEvent(_ =>
                {
                    if (node.Parent == null)
                        return;
                    if (hasStartedTransition)
                        return;

                    node.Parent.Replace(node, other);
                });
            }

            return node;
        }
    }
}
