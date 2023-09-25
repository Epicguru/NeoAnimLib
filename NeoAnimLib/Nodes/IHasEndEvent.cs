using System;

namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// An interface that nodes can implement that indicates that they have and End event that is raised whenever the end
    /// condition is met.
    /// </summary>
    public interface IHasEndEvent
    {
        /// <summary>
        /// Registers a callback that will be invoked whenever the end condition is met.
        /// </summary>
        void RegisterEndEvent(Action<AnimNode> endEvent);

        /// <summary>
        /// Un-registers a callback that would be invoked whenever the end condition is met.
        /// </summary>
        void UnRegisterEndEvent(Action<AnimNode> endEvent);
    }
}
