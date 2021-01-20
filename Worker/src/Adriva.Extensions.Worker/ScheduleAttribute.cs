using System;

namespace Adriva.Extensions.Worker
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ScheduleAttribute : Attribute
    {
        public string Expression { get; private set; }
        public Type ExpressionParserType { get; private set; }

        public ScheduleAttribute(string expression) : this(expression, typeof(CronExpressionParser))
        {

        }

        public ScheduleAttribute(string expression, Type expressionParserType)
        {
            this.Expression = expression;
            if (null == expressionParserType) throw new ArgumentNullException(nameof(expressionParserType), "Expression parser type is not set to an instance of an object.");
            if (!typeof(IExpressionParser).IsAssignableFrom(expressionParserType))
            {
                throw new InvalidCastException("Expression parser type should point to a class that implements the 'IExpressionParser' interface.");
            }
            this.ExpressionParserType = expressionParserType;
        }
    }
}
