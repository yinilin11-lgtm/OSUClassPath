using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OSUClassPath.Filters;

public sealed class AdminOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var isAdmin = context.HttpContext.Session.GetString("IsAdmin") == "true";

        if (isAdmin)
        {
            return;
        }

        var request = context.HttpContext.Request;
        var returnUrl = $"{request.PathBase}{request.Path}{request.QueryString}";
        context.Result = new RedirectToActionResult("Login", "Admin", new { returnUrl });
    }
}
