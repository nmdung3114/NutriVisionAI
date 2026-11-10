using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NutriVision.Api.Data;
using NutriVision.Api.Middlewares;
using NutriVision.Api.Services;
using NutriVision.Api.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Cấu hình DbContext (Hỗ trợ chuyển đổi giữa SQLite và SQL Server)
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
    ? builder.Configuration.GetConnectionString("SqlServerConnection")
    : builder.Configuration.GetConnectionString("SqliteConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Đăng ký Dependency Injection cho các tầng Dịch vụ (Services)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAiVisionService, AiVisionService>();
builder.Services.AddScoped<IMealService, MealService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// 3. Cấu hình JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "NutriVisionAI_Default_Secret_Key_Change_Me_Immediately!";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Cấu hình CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "http://localhost:3000", "http://localhost:3001" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 5. Cấu hình Controllers & Swagger OpenAPI với Authorization Bearer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NutriVisionAI API",
        Version = "v1",
        Description = "Hệ thống API Backend cho Nền tảng Dữ liệu & Ứng dụng Theo dõi Dinh dưỡng NutriVisionAI"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT Token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 6. Tự động khởi tạo CSDL và nạp dữ liệu mẫu (Seed Data)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.EnsureCreated();
        SeedData.InitializeAsync(dbContext).GetAwaiter().GetResult();
        Log.Information("Cơ sở dữ liệu đã được khởi tạo và nạp dữ liệu mẫu thành công.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Lỗi khi khởi tạo cơ sở dữ liệu");
    }
}

// 7. Pipeline cấu hình HTTP
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NutriVisionAI API v1");
    });
}

// Cấu hình thư mục lưu trữ ảnh tải lên (/uploads)
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseCors("AllowClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
