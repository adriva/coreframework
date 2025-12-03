using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace Adriva.Extensions.Runtime;

public interface IExpressionManager
{
    string Serialize(LambdaExpression expression, IRuntimeContext runtimeContext = null);

    LambdaExpression Deserialize(string text, IRuntimeContext runtimeContext = null);

    Expression<Func<TInput, TOutput>> CreateDynamicLambda<TInput, TOutput>(ParsingConfig parsingConfig, string expression, string parameterName = null, IRuntimeContext runtimeContext = null);

    Expression<Func<TArg1, TArg2, TOutput>> CreateDynamicLambda<TArg1, TArg2, TOutput>(ParsingConfig parsingConfig, string expression, string[] parameterNames = null, IRuntimeContext runtimeContext = null);

    Expression<Func<TInput, TOutput>> CreateDynamicLambda<TInput, TOutput>(string expression, string parameterName = null, IRuntimeContext runtimeContext = null);

    Expression<Func<TArg1, TArg2, TOutput>> CreateDynamicLambda<TArg1, TArg2, TOutput>(string expression, string[] parameterNames = null, IRuntimeContext runtimeContext = null);
}
