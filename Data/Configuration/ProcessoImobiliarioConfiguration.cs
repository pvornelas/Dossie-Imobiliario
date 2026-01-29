namespace DossieImobiliario.Data.Configuration;

public class ProcessoImobiliarioConfiguration : IEntityTypeConfiguration<ProcessoImobiliario>
{
    public void Configure(EntityTypeBuilder<ProcessoImobiliario> entity)
    {
        entity.ToTable("ProcessosImobiliarios");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.NumeroProcesso)
            .IsRequired()
            .HasMaxLength(30);

        entity.Property(e => e.Cliente)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Imovel)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.CriadoEm)
            .IsRequired();

        entity.HasIndex(e => e.NumeroProcesso).IsUnique();
        entity.HasIndex(e => e.Cliente);

        entity.HasMany(e => e.Documentos)
            .WithOne(d => d.Processo)
            .HasForeignKey(d => d.ProcessoImobiliarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
