namespace DossieImobiliario.Data.Configuration;

public class DocumentoProcessoConfiguration : IEntityTypeConfiguration<DocumentoProcesso>
{
    public void Configure(EntityTypeBuilder<DocumentoProcesso> entity)
    {
        entity.ToTable("DocumentosProcesso");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.TipoDocumento)
            .IsRequired()
            .HasMaxLength(80);

        entity.Property(x => x.NomeOriginal)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(x => x.NomeSalvo)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(x => x.CaminhoRelativo)
            .IsRequired()
            .HasMaxLength(400);

        entity.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(120);

        entity.Property(x => x.TamanhoBytes)
            .IsRequired();

        entity.Property(x => x.EnviadoEmUtc)
            .IsRequired();

        entity.HasIndex(x => x.ProcessoImobiliarioId);
    }
}
