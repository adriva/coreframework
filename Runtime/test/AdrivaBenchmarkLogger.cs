using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.EventProcessors;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace test;

internal sealed class AdrivaBenchmarkLogger : BenchmarkDotNet.Loggers.ILogger
{
    private static readonly LogKind[] IgnoredLogKinds = [
        LogKind.Hint,
        LogKind.Header,
        LogKind.Warning,
    ];

    private readonly ILogger BaseLogger = ConsoleLogger.Default;

    public string Id => nameof(AdrivaBenchmarkLogger);

    public int Priority => 0;

    public void Flush()
    {
        this.BaseLogger.Flush();
    }

    public void Write(LogKind logKind, string text)
    {
        if (AdrivaBenchmarkLogger.IgnoredLogKinds.Any(x => logKind == x)) return;
        this.BaseLogger.Write(text);
    }

    public void WriteLine()
    {
        this.BaseLogger.WriteLine();
    }

    public void WriteLine(LogKind logKind, string text)
    {
        if (AdrivaBenchmarkLogger.IgnoredLogKinds.Any(x => logKind == x)) return;
        this.BaseLogger.WriteLine(text);
    }
}
