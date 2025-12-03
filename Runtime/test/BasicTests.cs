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
        MethodCallRepository.BasicSerializationCall(expressionManager, originalExpression, false, this);
    }

    [TestMethod]
    public void TypedSimpleSerializeDeserialize()
    {
        var manager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
        Expression<Func<BasicTests, int>> expression = x => x.GetHashCode();

        var stringData = manager.Serialize(expression);
        var deserializedExpression = (Expression<Func<BasicTests, int>>)manager.Deserialize(stringData);

        Assert.AreEqual(expression.Compile()(this), expression.Compile()(this));
    }

    [TestMethod]
    public void AdvancedSerializeDeserialize1()
    {
        var expressionManager = this.ServiceProvider!.GetRequiredService<IExpressionManager>();
        LambdaExpression originalExpression = (BasicTests x) => Utilities.GetBaseString(BitConverter.GetBytes(DateTime.Today.AddDays(-1).Ticks), Utilities.Base36Alphabet);
        MethodCallRepository.AdvancedSerializationCall(expressionManager, originalExpression, false, this);
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
