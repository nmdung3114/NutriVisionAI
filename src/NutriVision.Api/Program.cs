using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NutriVision.Api.Common;
using NutriVision.Api.Filters;
using NutriVision.Api.Middleware;
using NutriVision.Application;
using NutriVision.Application.Abstractions;
using NutriVision.Application.Common;
using NutriVision.Application.Settings;
using NutriVision.Domain.Constants;
using NutriVision.Infrastructure;
using NutriVision.Infrastructure.Seed;
using NutriVision.Infrastructure.Settings;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/nutrivision-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14));

var config = builder.Configuration;
var jwt = config.GetSection(JwtSettings.Section).Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
    throw new InvalidOperationException("Cấu hình Jwt:Secret bắt buộc và phải dài ≥ 32 ký tự (đặt qua biến môi trường Jwt__Secret hoặc user-secrets).");
// Khóa ký URL ảnh: mặc định dẫn xuất từ Jwt:Secret nếu chưa cấu hình riêng.
if (string.IsNullOrWhiteSpace(config["Storage:SigningKey"])) config["Storage:SigningKey"] = "files:" + jwt.Secret;

builder.Services.Configure<AppSettings>(config.GetSection(AppSettings.Section));
builder.Services.Configure<AuthPolicySettings>(config.GetSection(AuthPolicySettings.Section));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);

builder.Services.AddControllers(o => o.Filters.Add<ValidationFilter>())
    .ConfigureApiBehaviorOptions(o =>
    {
        // JSON sai định dạng / kiểu dữ liệu sai → envelope thống nhất.
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var fields = ctx.ModelState.Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(kv => string.IsNullOrEmpty(kv.Key) ? "body" : kv.Key.TrimStart('$', '.'),
                    kv => kv.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Giá trị không hợp lệ." : "Giá trị không hợp lệ.").Distinct().ToArray());
            var ex = new ValidationAppException(fields);
            return new BadRequestObjectResult(ApiResponse.Fail(ex.Code, ex.Message, ex.Payload, ex.Errors));
        };
    });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt.Issuer,
            ValidateAudience = true, ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30),
        };
        o.Events = new JwtBearerEvents
        {
            // Tài khoản bị khóa / đổi vai trò có hiệu lực ngay, không chờ access token hết hạn.
            OnTokenValidated = async ctx =>
            {
                var idStr = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var role = ctx.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (!Guid.TryParse(idStr, out var id)) { ctx.Fail("invalid"); return; }
                var db = ctx.HttpContext.RequestServices.GetRequiredService<IApplicationDbContext>();
                var ok = await db.NguoiDungs.AsNoTracking().AnyAsync(u => u.Id == id && u.TrangThai == TrangThaiTaiKhoan.Active && u.VaiTro == role);
                if (!ok) ctx.Fail("Tài khoản không còn hợp lệ.");
            },
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                await WriteJson(ctx.HttpContext, 401, ApiResponse.Fail(ErrorCodes.Unauthorized, "Bạn cần đăng nhập để tiếp tục."));
            },
            OnForbidden = ctx => WriteJson(ctx.HttpContext, 403, ApiResponse.Fail(ErrorCodes.Forbidden, "Bạn không có quyền truy cập chức năng này.")),
        };
    });
builder.Services.AddAuthorization();

var cors = config.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(cors).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Content-Disposition")));

var authPerMinute = config.GetValue("RateLimit:AuthPerMinute", 20);
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = 429;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = authPerMinute, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    o.OnRejected = (ctx, _) => new ValueTask(WriteJson(ctx.HttpContext, 429,
        ApiResponse.Fail(ErrorCodes.RateLimited, "Bạn thao tác quá nhanh. Vui lòng thử lại sau ít phút.")));
});

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "NutriVision AI API", Version = "v1" });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header,
        Description = "Nhập access token (không cần tiền tố 'Bearer ').",
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [],
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing")) app.UseHsts();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

await DbInitializer.InitializeAsync(app.Services);
await app.RunAsync();

static Task WriteJson(HttpContext ctx, int status, object body)
{
    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/json; charset=utf-8";
    return ctx.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(body, ExceptionHandlingMiddleware.JsonOptions));
}

public partial class Program;
