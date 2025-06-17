using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Adriva.Common.Core;
using Adriva.Extensions.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace test;

[TestClass]
public sealed class BasicTests
{
    private IServiceProvider? ServiceProvider;

    [TestInitialize]
    public void Initialize()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddExpressionManager().ConfigureDefaultContext(c =>
        {
            c.AddKnownType(typeof(Adriva.Common.Core.Utilities));
        });

        this.ServiceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public void SimpleSerializeDeserialize()
    {
        var expressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
        LambdaExpression originalExpression = (BasicTests x) => x.GetHashCode();
        var stringData = expressionManager.Serialize(originalExpression);
        var expression = expressionManager.Deserialize(stringData);

        Assert.AreEqual(originalExpression.Compile().DynamicInvoke(this), expression.Compile().DynamicInvoke(this));
    }

    [TestMethod]
    public void AdvancedSerializeDeserialize1()
    {
        var expressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
        LambdaExpression originalExpression = (BasicTests x) => Utilities.GetBaseString(BitConverter.GetBytes(DateTime.Today.AddDays(-1).Ticks), Utilities.Base36Alphabet);
        var serializedData = expressionManager.Serialize(originalExpression);
        var expression = expressionManager.Deserialize(serializedData);

        Assert.AreEqual(originalExpression.Compile().DynamicInvoke(this), expression.Compile().DynamicInvoke(this));
    }

    [TestMethod]
    public void CreateAndRunBasicLambda()
    {
        var expressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();

        var lambda = expressionManager.CreateDynamicLambda<BasicTests, string>("Utilities.GetBaseString(ulong(x.GetHashCode()), Utilities.Base36Alphabet)", "x");

        Assert.AreEqual(Utilities.GetBaseString((ulong)this.GetHashCode(), Utilities.Base36Alphabet), lambda.Compile().Invoke(this));
    }

    [TestMethod]
    public void CreateAndRun2ArgLambda()
    {
        var expressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();

        var lambda = expressionManager.CreateDynamicLambda<long, long, long>("x+y", ["x", "y"]);

        Assert.AreEqual(12 + 9, lambda.Compile().Invoke(12, 9));
    }
}
