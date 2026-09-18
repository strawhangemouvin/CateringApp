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

            // 0. Cek apakah aksi atau controller memiliki atribut [AllowAnonymous]
            if (context.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any())
            {
                base.OnActionExecuting(context);
                return;
            }

            var session = httpContext.Session;
            int? sessionUserId = session.GetInt32("UserId");
            string? sessionRole = session.GetString("Role");

            // 1. Cek apakah session aktif sudah ada (Prioritas Utama untuk aplikasi web)
            if (sessionUserId.HasValue && !string.IsNullOrEmpty(sessionRole))
            {
                if (_roles != null && _roles.Length > 0)
                {
                    if (!_roles.Any(r => r.Equals(sessionRole, StringComparison.OrdinalIgnoreCase)))
                    {
                        // 403 Forbidden sesuai kebutuhan error handling
                        context.Result = new ViewResult
                        {
                            ViewName = "Error403",
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }

                base.OnActionExecuting(context);
                return;
            }

            // 2. Jika session belum terisi (misal browser baru dibuka), pulihkan dari Cookie JwtToken
            var token = httpContext.Request.Cookies["JwtToken"];
            if (!string.IsNullOrEmpty(token))
            {
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
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                    var jwtToken = (JwtSecurityToken)validatedToken;

                    var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
                                      ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                    var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value 
                                        ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value 
                                        ?? "user";
                    var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value 
                                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value 
                                    ?? "User";
                    var fullNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value 
                                        ?? usernameClaim;

                    if (!string.IsNullOrEmpty(userIdClaim))
                    {
                        session.SetInt32("UserId", int.Parse(userIdClaim));
                        session.SetString("Username", usernameClaim);
                        session.SetString("Nama", fullNameClaim);
                        session.SetString("Role", roleClaim);

                        if (_roles != null && _roles.Length > 0)
                        {
                            if (!_roles.Any(r => r.Equals(roleClaim, StringComparison.OrdinalIgnoreCase)))
                            {
                                context.Result = new ViewResult
                                {
                                    ViewName = "Error403",
                                    StatusCode = StatusCodes.Status403Forbidden
                                };
                                return;
                            }
                        }

                        base.OnActionExecuting(context);
                        return;
                    }
                }
                catch
                {
                    httpContext.Response.Cookies.Delete("JwtToken");
                    session.Clear();
                }
            }

            // 3. Jika tidak ada Session dan tidak ada Token valid:
            httpContext.Response.Cookies.Delete("JwtToken");
            session.Clear();

            var isAjax = httpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                         httpContext.Request.Headers.Accept.ToString().Contains("application/json");
            if (isAjax)
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error401",
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (context.Controller is Controller controller)
            {
                if (controller.TempData["Warning"] == null && controller.TempData["Error"] == null)
                {
                    controller.TempData["Warning"] = "Silakan masuk ke akun Anda terlebih dahulu untuk melanjutkan.";
                }
            }

            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}
