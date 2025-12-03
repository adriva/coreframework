namespace Adriva.Extensions.Analytics.Server.Entities
{
    /// <summary>
    /// Represents the severity level of an analytics item.
    /// </summary>
    public enum Severity
    {
        //
        // Summary:
        //     Verbose severity level.
        Verbose = 0,
        //
        // Summary:
        //     Information severity level.
        Information = 1,
        //
        // Summary:
        //     Warning severity level.
        Warning = 2,
        //
        // Summary:
        //     Error severity level.
        Error = 3,
        //
        // Summary:
        //     Critical severity level.
        Critical = 4
    }

    public enum DataPointType
    {
        Measurement,
        Aggregation,
    }
}
