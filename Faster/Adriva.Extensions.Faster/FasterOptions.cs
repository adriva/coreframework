using System;
using Microsoft.AspNetCore.Http;
namespace Adriva.Extensions.Faster
{
    public class FasterOptions
    {
        private const long Limit16Gb = 17179869184L;
        private const long Limit1Gb = 1073741824L;

        public bool UseLocks { get; set; }

        /// <summary>
        /// <remarks>The system will set this limit to 1GB if set to a lower limit by the user.</remarks>
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// <remarks>The system will set this limit to 512MB if set to a lower limit by the user.</remarks>
        /// </summary>
        public long MemoryCacheLimit { get; set; }

        /// <summary>
        /// Gets or sets the base path for cache operations.
        /// <remarks>PathBase should start with a / character.</remarks>
        /// </summary>
        public PathString PathBase { get; set; } = "/faster";

        internal void Normalize()
        {
            if (string.IsNullOrWhiteSpace(this.PathBase))
            {
                this.PathBase = "/faster";
            }

            this.MemoryLimit = Math.Max(FasterOptions.Limit1Gb, Math.Min(FasterOptions.Limit16Gb, this.MemoryLimit));
            this.MemoryCacheLimit = Math.Max(FasterOptions.Limit1Gb / 2, Math.Min(FasterOptions.Limit16Gb, this.MemoryLimit));
        }
    }
}