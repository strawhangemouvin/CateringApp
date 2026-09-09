using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CateringApp.Middleware
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (!context.Response.HasStarted && IsApiRequest(context))
                {
                    int statusCode = context.Response.StatusCode;
                    if (statusCode is 400 or 401 or 403 or 404 or 422 or 500)
                    {
                        string message = statusCode switch
                        {
                            400 => "Bad Request. Format atau parameter permintaan tidak valid.",
                            401 => "Unauthorized. Token otentikasi tidak valid atau belum disertakan.",
                            403 => "Forbidden. Anda tidak memiliki hak akses (role) untuk resource ini.",
                            404 => "Not Found. Resource atau endpoint yang diminta tidak ditemukan.",
                            422 => "Unprocessable Entity. Terjadi kesalahan validasi data input.",
                            _ => "Internal Server Error. Terjadi kesalahan internal pada server."
                        };

                        var errorResponse = new
                        {
                            statusCode = statusCode,
                            status = statusCode >= 500 ? "error" : "fail",
                            message = message
                        };

                        context.Response.ContentType = "application/json";
                        var json = JsonSerializer.Serialize(errorResponse);
                        await context.Response.WriteAsync(json);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception on {context.Request.Method} {context.Request.Path}: {ex.Message}");

                if (!context.Response.HasStarted)
                {
                    if (IsApiRequest(context))
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "application/json";

                        var errorResponse = new
                        {
                            statusCode = 500,
                            status = "error",
                            message = "Terjadi kesalahan internal pada server.",
                            details = _env.IsDevelopment() ? ex.Message : null
                        };

                        var json = JsonSerializer.Serialize(errorResponse);
                        await context.Response.WriteAsync(json);
                    }
                    else
                    {
                        
                        context.Response.Redirect("/Home/ErrorStatus/500");
                    }
                }
            }
        }

        private static bool IsApiRequest(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            var accept = context.Request.Headers["Accept"].ToString();
            var requestedWith = context.Request.Headers["X-Requested-With"].ToString();

            return path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
                   accept.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
                   requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}
