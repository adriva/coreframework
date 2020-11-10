using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportingServiceOptions
    {
        public bool UseCache { get; set; }

        public TimeSpan? DefinitionTimeToLive { get; set; }
    }
}