using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Adriva.Web.Common
{
    public static class ActionContextExtensions
    {
        public static IEnumerable<string> GetValidationErrorMessages(this ActionContext actionContext)
        {
            if (null == actionContext) yield break;
            if (actionContext.ModelState.IsValid) yield break;

            foreach (var value in actionContext.ModelState.Values)
            {
                if (null != value.Errors)
                {
                    foreach (var error in value.Errors)
                    {
                        yield return error.ErrorMessage;
                    }
                }
            }
        }
    }
}