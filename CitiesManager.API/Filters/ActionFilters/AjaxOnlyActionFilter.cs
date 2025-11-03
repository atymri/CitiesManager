using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CitiesManager.API.Filters.ActionFilters
{
    public class AjaxOnlyActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;

            if (!headers.TryGetValue("x-requested-from", out var value) ||
                !string.Equals("xmlhttprequest", value, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }

    }
}
