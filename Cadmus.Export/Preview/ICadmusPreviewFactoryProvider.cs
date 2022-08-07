namespace Cadmus.Export.Preview
{
    /// <summary>
    /// Provider for <see cref="CadmusPreviewFactory"/>.
    /// </summary>
    public interface ICadmusPreviewFactoryProvider
    {
        /// <summary>
        /// Gets the factory.
        /// </summary>
        /// <param name="profile">The profile.</param>
        /// <returns>Factory.</returns>
        CadmusPreviewFactory GetFactory(string profile);
    }
}
