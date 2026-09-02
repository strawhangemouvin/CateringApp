using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

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
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Cookies["JwtToken"];

            if (string.IsNullOrEmpty(token))
            {
                // No token found in Cookie, redirect to Login
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
                
                var jwtSettings = configuration.GetSection("Jwt");
                var key = jwtSettings["Key"] ?? "CateringAppSuperSecretKeyForJwtAuthenticationServiceNet8";
                var issuer = jwtSettings["Issuer"] ?? "CateringApp";
                var audience = jwtSettings["Audience"] ?? "CateringAppUsers";

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the JWT Token
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // Extract Claims
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                                  ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value 
                                  ?? string.Empty;
                var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value 
                                    ?? "user";
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value 
                                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value 
                                ?? "User";
                var fullNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value 
                                    ?? usernameClaim;

                if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim) || userIdClaim == string.Empty || roleClaim == "User")
                {
                    // Check if it's actually valid
                    if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(roleClaim))
                    {
                        throw new Exception("Klaim penting tidak ditemukan dalam token.");
                    }
                }

                // Restore/sync standard session cache for views/layouts compatibility
                var session = httpContext.Session;
                if (session.GetInt32("UserId") == null)
                {
                    session.SetInt32("UserId", int.Parse(userIdClaim));
                    session.SetString("Username", usernameClaim ?? "user");
                    session.SetString("Nama", fullNameClaim);
                    session.SetString("Role", roleClaim);
                }

                // Enforce Role authorization if roles are specified
                if (_roles != null && _roles.Length > 0)
                {
                    if (!_roles.Any(r => r.Equals(roleClaim, StringComparison.OrdinalIgnoreCase)))
                    {
                        // Redirect to Access Denied page
                        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // Token invalid, expired, or validation failed -> Clear Cookie & Session, then redirect to Login
                httpContext.Response.Cookies.Delete("JwtToken");
                httpContext.Session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
