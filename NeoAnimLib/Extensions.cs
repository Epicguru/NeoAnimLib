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
    }
}
