using System;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Serialize.Linq.Serializers;

namespace Adriva.Extensions.Runtime;

public class ExpressionManager(IServiceProvider serviceProvider) : IExpressionManager
{
    private readonly JsonSerializer Serializer = new();

    protected IServiceProvider ServiceProvider { get; } = serviceProvider;

    private LambdaExpression CreateDynamicLambda<TOutput>(ParsingConfig parsingConfig, string expression, Tuple<Type, string>[] arguments, IRuntimeContext runtimeContext = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentNullException(nameof(expression), "Expression text is not set.");
        }

        ParameterExpression[] parameters = new ParameterExpression[arguments.Length];

        for (int loop = 0; loop < arguments.Length; loop++)
        {
            parameters[loop] = Expression.Parameter(arguments[loop].Item1, arguments[loop].Item2);
        }

        parsingConfig.CustomTypeProvider = ActivatorUtilities.CreateInstance<DefaultDynamicLinqTypeProvider>(this.ServiceProvider, parsingConfig);

        try
        {
            return DynamicExpressionParser.ParseLambda(parsingConfig, parameters, typeof(TOutput), expression);
        }
        catch (ParseException parseException)
        {
            throw new ParseException($"Parsing expression '{expression}' failed. ({parseException.Message})", parseException);
        }
    }

    public virtual string Serialize(LambdaExpression expression, IRuntimeContext runtimeContext = null)
    {
        if (expression is null)
        {
            return null;
        }

        runtimeContext ??= this.ServiceProvider.GetRequiredService<IRuntimeContext>();

        ExpressionSerializer expressionSerializer = new(this.Serializer);
        expressionSerializer.AddKnownTypes(runtimeContext.KnownTypes);

        return expressionSerializer.SerializeText(expression, new()
        {
            UseRelaxedTypeNames = false
        });
    }

    public virtual LambdaExpression Deserialize(string text, IRuntimeContext runtimeContext = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        runtimeContext ??= this.ServiceProvider.GetRequiredService<IRuntimeContext>();

        ExpressionSerializer expressionSerializer = new(this.Serializer);
        expressionSerializer.AddKnownTypes(runtimeContext.KnownTypes);

        return (LambdaExpression)expressionSerializer.DeserializeText(text);
    }

    public Expression<Func<TInput, TOutput>> CreateDynamicLambda<TInput, TOutput>(string expression, string parameterName = null, IRuntimeContext runtimeContext = null)
    {
        return this.CreateDynamicLambda<TInput, TOutput>(ParsingConfig.Default, expression, parameterName, runtimeContext);
    }

    public Expression<Func<TArg1, TArg2, TOutput>> CreateDynamicLambda<TArg1, TArg2, TOutput>(string expression, string[] parameterNames = null, IRuntimeContext runtimeContext = null)
    {
        return this.CreateDynamicLambda<TArg1, TArg2, TOutput>(ParsingConfig.Default, expression, parameterNames, runtimeContext);
    }

    public virtual Expression<Func<TInput, TOutput>> CreateDynamicLambda<TInput, TOutput>(ParsingConfig parsingConfig, string expression, string parameterName = null, IRuntimeContext runtimeContext = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentNullException(nameof(expression), "Expression text is not set.");
        }

        return (Expression<Func<TInput, TOutput>>)this.CreateDynamicLambda<TOutput>(parsingConfig, expression, [
            new Tuple<Type, string>(typeof(TInput), parameterName)
        ], runtimeContext);
    }

    public virtual Expression<Func<TArg1, TArg2, TOutput>> CreateDynamicLambda<TArg1, TArg2, TOutput>(ParsingConfig parsingConfig, string expression, string[] parameterNames = null, IRuntimeContext runtimeContext = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentNullException(nameof(expression), "Expression text is not set.");
        }

        if (parameterNames is null || 2 != parameterNames.Length)
        {
            throw new ArgumentException("ParameterNames parameter should be set to an array of exactly 2 items.");
        }

        return (Expression<Func<TArg1, TArg2, TOutput>>)this.CreateDynamicLambda<TOutput>(parsingConfig, expression, [
            new Tuple<Type, string>(typeof(TArg1), parameterNames?[0]),
            new Tuple<Type, string>(typeof(TArg2), parameterNames?[1])
        ], runtimeContext);
    }


}