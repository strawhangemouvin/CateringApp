using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace CateringApp.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");

            if (userId == null)
            {
                // Redirect to Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_roles != null && _roles.Length > 0)
            {
                var role = session.GetString("Role");
                if (role == null || !_roles.Any(r => r.Equals(role, System.StringComparison.OrdinalIgnoreCase)))
                {
                    // Redirect to Access Denied
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
