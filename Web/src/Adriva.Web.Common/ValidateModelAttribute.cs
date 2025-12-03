using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adriva.Web.Common
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private bool IncludeErrorMessage;
        public HttpStatusCode InvalidModelStatusCode { get; set; } = HttpStatusCode.BadRequest;

        public ValidateModelAttribute(bool includeErrorMessage)
        {
            this.IncludeErrorMessage = includeErrorMessage;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                if (!this.IncludeErrorMessage)
                {
                    context.Result = new StatusCodeResult((int)this.InvalidModelStatusCode);
                }
                else
                {
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
            }
        }
    }
}