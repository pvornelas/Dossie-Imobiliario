namespace DossieImobiliario.Models;

public class DocumentoProcesso
{
    public int Id { get; set; }

    public int ProcessoImobiliarioId { get; set; }
    public ProcessoImobiliario Processo { get; set; } = default!;
    public string TipoDocumento { get; set; } = default!;
    public string NomeOriginal { get; set; } = default!;
    public string NomeSalvo { get; set; } = default!;
    public string CaminhoRelativo { get; set; } = default!;
    public string ContentType { get; set; } = "application/octet-stream";
    public long TamanhoBytes { get; set; }

    public DateTime EnviadoEmUtc { get; set; } = DateTime.UtcNow;
}
