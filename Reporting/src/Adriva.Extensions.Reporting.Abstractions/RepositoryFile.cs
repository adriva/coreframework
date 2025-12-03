using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("RepositoryFile = {Name}")]
    public struct RepositoryFile
    {
        [NonSerialized]
        [JsonIgnore]
        private static readonly RepositoryFile NotExistsFile = new() { Name = null, Path = null, Base = null };

        [JsonIgnore]
        public static RepositoryFile NotExists => RepositoryFile.NotExistsFile;

        public string Name { get; set; }

        public string Path { get; set; }

        public string Base { get; set; }
    }
}
