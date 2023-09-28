using NeoAnimLib.Nodes;
using System;

namespace NeoAnimLib
{
    /// <summary>
    /// Represents an animation event that is part of a clip.
    /// Simply a structure with a time and a callback method.
    /// </summary>
    public struct AnimEvent
    {
        /// <summary>
        /// The time of this event, in seconds.
        /// This time is expected to be between 0 and <see cref="IAnimClip.Length"/>, inclusive.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// The action that is raised when this event is hit.
        /// </summary>
        public Action<ClipAnimNode>? Action { get; set; }

        /// <summary>
        /// Constructs a new anim event given a time and an action to raise.
        /// </summary>
        public AnimEvent(float time, Action<ClipAnimNode>? action)
        {
            Time = time;
            Action = action;
        }
    }
}
