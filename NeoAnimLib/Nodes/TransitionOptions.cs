namespace NeoAnimLib.Nodes
{
    /// <summary>
    /// Options that can be set when calling <see cref="AnimNode.TransitionTo"/>.
    /// </summary>
    public struct TransitionOptions
    {
        /// <summary>
        /// If true, the <see cref="AnimNode.LocalTime"/> and <see cref="AnimNode.LocalSpeed"/>
        /// of the target node are set to match the existing node.
        /// </summary>
        public bool SyncTime { get; set; }
    }
}
