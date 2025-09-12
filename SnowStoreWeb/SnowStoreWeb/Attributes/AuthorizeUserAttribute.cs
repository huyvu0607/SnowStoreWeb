using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SnowStoreWeb.Attributes
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        private readonly string _role;

        public AuthorizeUserAttribute(string role = "")
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var userId = httpContext.Session.GetString("UserId");
            var userRole = httpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập → redirect Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!string.IsNullOrEmpty(_role) && userRole != _role)
            {
                // Nếu có yêu cầu role cụ thể mà user không đủ quyền → chặn
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
