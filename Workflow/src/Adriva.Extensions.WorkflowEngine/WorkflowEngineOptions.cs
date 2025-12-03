using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public class WorkflowEngineOptions
    {
        public IEnumerable<Type> CustomTypes { get; set; }

        /// <summary>
        /// Gets or sets the cache duration for compiled workflows in minutes.
        /// <remarks>0 (zero) or a negative number disables the cache.</remarks>
        /// </summary>
        public int CompilationCacheDuration { get; set; }
    }
}
