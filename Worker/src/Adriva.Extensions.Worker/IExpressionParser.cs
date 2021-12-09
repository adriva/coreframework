using System;

namespace Adriva.Extensions.Worker
{
    public interface IExpressionParser
    {
        DateTime? GetNext(DateTime fromDate, string expression);
    }
}
