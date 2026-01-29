namespace DossieImobiliario.Extensions;
public static class DatabaseExtensions
{
    public static IServiceCollection AddSqliteDatabase(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        var cs = config.GetConnectionString("Database")!;
        var dataSource = new SqliteConnectionStringBuilder(cs).DataSource;

        var caminhoDb = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(env.ContentRootPath, dataSource);

        var dirDb = Path.GetDirectoryName(caminhoDb);
        if (!string.IsNullOrWhiteSpace(dirDb))
            Directory.CreateDirectory(dirDb);

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(cs));
        return services;
    }
}