using DossieImobiliario.Models;
using Microsoft.EntityFrameworkCore;

namespace DossieImobiliario.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<ProcessoImobiliario> Processos => Set<ProcessoImobiliario>();
    public DbSet<DocumentoProcesso> Documentos => Set<DocumentoProcesso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
