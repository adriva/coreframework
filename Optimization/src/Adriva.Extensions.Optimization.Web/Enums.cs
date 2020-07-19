namespace Adriva.Extensions.Optimization.Web
{
    /// <summary>
    /// Specifies how the optimized assets will be delivered.
    /// </summary>
    public enum OptimizationTagOutput : int
    {
        /// <summary>
        /// Delivers the optimized asset using the default mode.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Delivers the content of the asset content inline.
        /// </summary>
        Inline = 0,

        /// <summary>
        /// Delivers the content of the asset in a seperate content file and serve it as a static file.
        /// </summary>
        StaticFile = 2,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        Loader = 3,

        /// <summary>
        /// Uses the source path to generate an asset tag.
        /// </summary>
        Tag = 4,
    }
}