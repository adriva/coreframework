using System.Linq.Expressions;
using Adriva.Common.Core;
using Adriva.Extensions.Runtime;

namespace test;

internal static class MethodCallRepository
{
    public static void BasicSerializationCall(IExpressionManager manager, LambdaExpression expression, bool noAssert, object arg1)
    {
        var stringData = manager.Serialize(expression);
        var deserializedExpression = manager.Deserialize(stringData);

        if (!noAssert)
        {
            Assert.AreEqual(expression.Compile().DynamicInvoke(arg1), deserializedExpression.Compile().DynamicInvoke(arg1));
        }
    }

    public static void TypedSimpleSerializeDeserializeCall<TIn, TOut>(IExpressionManager manager, Expression<Func<TIn, TOut>> expression, bool noAssert, TIn arg1)
    {
        var stringData = manager.Serialize(expression);
        var deserializedExpression = (Expression<Func<TIn, TOut>>)manager.Deserialize(stringData);

        if (!noAssert)
        {
            Assert.AreEqual(expression.Compile()(arg1), deserializedExpression.Compile()(arg1));
        }
    }

    public static void AdvancedSerializationCall(IExpressionManager manager, LambdaExpression expression, bool noAssert, object arg1)
    {
        LambdaExpression originalExpression = (BasicTests x) => Utilities.GetBaseString(BitConverter.GetBytes(DateTime.Today.AddDays(-1).Ticks), Utilities.Base36Alphabet);
        var serializedData = manager.Serialize(originalExpression);
        var deserializedExpression = manager.Deserialize(serializedData);

        if (!noAssert)
        {
            Assert.AreEqual(originalExpression.Compile().DynamicInvoke(arg1), expression.Compile().DynamicInvoke(arg1));
        }
    }
}
