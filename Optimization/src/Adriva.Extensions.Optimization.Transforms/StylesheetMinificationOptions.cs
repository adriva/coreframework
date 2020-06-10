using System.Collections.Generic;

namespace Adriva.Extensions.Optimization.Transforms
{
    /// <summary>
    /// Provides configuration options that may be used in Stylesheet transforms.
    /// </summary>
    public class StylesheetMinificationOptions
    {
        /// <summary>
        /// Gets a dictionary of token and value pairs which may be used to replace stylesheet tokens with their values in the optimization pipeline.
        /// </summary>
        /// <typeparam name="string">The token that will be replaced.</typeparam>
        /// <typeparam name="string">The value that will overwrite the token value.</typeparam>
        /// <returns>A concrete implementation of IDictionary&lt;string, string&gt; interface to store the substitutions.</returns>
        public IDictionary<string, string> Substitutions { get; private set; } = new Dictionary<string, string>();
    }
}
