using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NutriVision.Application.Common;
using NutriVision.Application.Modules.Admin;
using NutriVision.Application.Modules.Audit;
using NutriVision.Application.Modules.Auth;
using NutriVision.Application.Modules.Dashboard;
using NutriVision.Application.Modules.Foods;
using NutriVision.Application.Modules.Habits;
using NutriVision.Application.Modules.Meals;
using NutriVision.Application.Modules.Profile;
using NutriVision.Application.Modules.Reports;
using NutriVision.Application.Modules.Suggestions;
using NutriVision.Application.Modules.Tracking;

namespace NutriVision.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingletonTimeProvider();
        services.AddSingleton<AppClock>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>(ServiceLifetime.Singleton);

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IFoodCatalog, FoodCatalog>();
        services.AddScoped<IFoodService, FoodService>();
        services.AddScoped<IComponentNutritionService, ComponentNutritionService>();
        services.AddScoped<IDailySummaryService, DailySummaryService>();
        services.AddScoped<MealMapper>();
        services.AddScoped<IMealService, MealService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IHabitService, HabitService>();
        services.AddScoped<IWeightService, WeightService>();
        services.AddScoped<ISuggestionService, SuggestionService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }

    private static void TryAddSingletonTimeProvider(this IServiceCollection services)
    {
        if (services.All(d => d.ServiceType != typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);
    }
}
