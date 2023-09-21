namespace NeoAnimLib
{
    /// <summary>
    /// A delegate that takes in the path of a property
    /// and should return the default value for said property.
    /// </summary>
    public delegate float DefaultValueSource(string propertyPath);
}
