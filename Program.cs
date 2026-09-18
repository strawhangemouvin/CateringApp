using CateringApp.Models.Entity;
using CateringApp.Services.Context;
using CateringApp.Services.Impl;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CateringDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICateringService, CateringService>();
builder.Services.AddScoped<IPasswordHasher<Pengguna>, PasswordHasher<Pengguna>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"] ?? "CateringAppSuperSecretKeyForJwtAuthenticationServiceNet8";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "CateringApp",
        ValidAudience = jwtSettings["Audience"] ?? "CateringAppUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var frontendWwwroot = Path.Combine(builder.Environment.ContentRootPath, "frontend", "wwwroot");
if (Directory.Exists(frontendWwwroot))
{
    builder.Environment.WebRootPath = frontendWwwroot;
}

builder.Services.AddControllersWithViews(options =>
{
    // Lokalisasi Pesan Validasi Model Binding ke Bahasa Indonesia
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        x => $"Kolom {x} harus berupa angka yang valid.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (val, x) => $"Nilai '{val}' tidak valid untuk {x}.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "Kolom ini wajib diisi.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        x => $"Nilai untuk kolom '{x}' tidak boleh kosong.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        x => $"Kolom {x} wajib diisi.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        x => $"Nilai '{x}' tidak valid.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
        x => $"Nilai tidak valid untuk kolom {x}.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        val => $"Nilai '{val}' tidak valid.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => "Nilai yang dimasukkan tidak valid.");
    options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(
        () => "Kolom harus berupa angka yang valid.");
})
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/frontend/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/frontend/Views/Shared/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new
            {
                statusCode = 400,
                status = "fail",
                message = "Validasi data input gagal.",
                errors = errors
            };

            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CateringApp REST API",
        Version = "v1",
        Description = "Dokumentasi Lengkap API CateringApp - Mendukung pengujian seluruh endpoint dengan Autentikasi JWT Bearer."
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Masukkan token JWT yang didapatkan dari POST /api/auth/login. Contoh: {token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CateringApp API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<CateringApp.Middleware.GlobalErrorHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/ErrorStatus/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
if (Directory.Exists(frontendWwwroot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendWwwroot),
        RequestPath = ""
    });
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' https: data: 'unsafe-inline' 'unsafe-eval';");
    await next();
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CateringDbContext>();
        CateringApp.Services.Context.DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Terjadi kesalahan saat seeding database.");
    }
}

app.Run();
