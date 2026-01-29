namespace DossieImobiliario.Extensions;

public static class CorsExtensions
{
    public const string PolicyDev = "Dev";
    public const string PolicyProd = "Prod";

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services, IConfiguration config)
    {
        var origensProd = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyDev, policy =>
            {
                policy
                    .WithOrigins(
                        "https://localhost:7143",
                        "http://localhost:5075",
                        "http://localhost:5075"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            options.AddPolicy(PolicyProd, policy =>
            {
                policy
                    .WithOrigins(origensProd)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCorsByEnvironment(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        return env.IsDevelopment()
            ? app.UseCors(PolicyDev)
            : app.UseCors(PolicyProd);
    }
}