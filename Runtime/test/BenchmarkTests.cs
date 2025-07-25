using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Adriva.Common.Core;
using Adriva.Extensions.Runtime;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.EventProcessors;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace test;

[TestClass]
public class BenchmarkTests
{
    private IServiceProvider? ServiceProvider;

    private IExpressionManager? ExpressionManager;

    [GlobalSetup]
    [MemberNotNull(nameof(ExpressionManager))]
    public void Initialize()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddExpressionManager().ConfigureDefaultContext(c =>
        {
            c.AddKnownType(typeof(Adriva.Common.Core.Utilities));
        });

        this.ServiceProvider = services.BuildServiceProvider();
        this.ExpressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
    }

    [Benchmark]
    [InvocationCount(20)]
    [IterationCount(20)]
    public void BenchmarkSimpleSerialization()
    {
        LambdaExpression expression = (BasicTests x) => x.GetHashCode();
        BasicTests basicTests = new();
        MethodCallRepository.BasicSerializationCall(this.ExpressionManager!, expression, true, basicTests);
    }

    [Benchmark]
    [InvocationCount(20)]
    [IterationCount(20)]
    public void BenchmarkTypedSimpleSerialization()
    {
        var manager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
        BasicTests basicTests = new();
        Expression<Func<BasicTests, int>> expression = x => x.GetHashCode();

        var stringData = manager.Serialize(expression);
        _ = (Expression<Func<BasicTests, int>>)manager.Deserialize(stringData);
    }

    [TestMethod]
    public void RunBenchmarks()
    {
        var manualConfig = ManualConfig.CreateEmpty();

        manualConfig.AddJob(Job.Dry);

        manualConfig.AddColumnProvider(DefaultColumnProviders.Instance);
        manualConfig.AddAnalyser([.. DefaultConfig.Instance.GetAnalysers()]);
        manualConfig.AddValidator([.. DefaultConfig.Instance.GetValidators()]);

        // manualConfig.AddLogger(ConsoleLogger.Default);
        manualConfig.AddLogger(NullLogger.Instance);
        // manualConfig.AddAnalyser(new AdrivaBenchmarkAnalyzer());

        manualConfig.WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.DisableOptimizationsValidator);
        manualConfig.WithUnionRule(ConfigUnionRule.Union);

        var benchmarkConfig = ImmutableConfigBuilder.Create(manualConfig);

        var summaries = BenchmarkRunner.Run(typeof(BenchmarkTests).Assembly, benchmarkConfig);

        summaries.ForEach((_, summary) =>
        {
            // summary.Table.Columns.ForEach((_, x) => System.Console.WriteLine(x));

            var methodColumn = summary.Table.Columns.FirstOrDefault(c => "Method".Equals(c.Header, StringComparison.Ordinal));
            var meanColumn = summary.Table.Columns.FirstOrDefault(c => "Mean".Equals(c.Header, StringComparison.Ordinal));
            var iterationCountColumn = summary.Table.Columns.FirstOrDefault(c => "IterationCount".Equals(c.Header, StringComparison.Ordinal));

            if (methodColumn is not null && meanColumn is not null && iterationCountColumn is not null)
            {
                if (methodColumn.Content.Length == meanColumn.Content.Length && iterationCountColumn.Content.Length == meanColumn.Content.Length)
                {
                    ConsoleLogger.Default.WriteLine($"{methodColumn.Header.PadRight(40)}{meanColumn.Header.PadLeft(18)}    {iterationCountColumn.Header}");
                    ConsoleLogger.Default.WriteLine($"{"-".PadRight(76, '-')}");

                    for (var loop = 0; loop < methodColumn.Content.Length; loop++)
                    {
                        ConsoleLogger.Default.WriteLine($"{methodColumn.Content[loop].PadRight(40)}{meanColumn.Content[loop].PadLeft(18)}\t{iterationCountColumn.Content[loop]}");
                    }
                }
            }
        });

    }
}
