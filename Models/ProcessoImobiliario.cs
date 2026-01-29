namespace DossieImobiliario.Models;

public class ProcessoImobiliario
{
    public int Id { get; set; }

    public string NumeroProcesso { get; set; } = default!;
    public string Cliente { get; set; } = default!;
    public string Imovel { get; set; } = default!;

    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;

    public List<DocumentoProcesso> Documentos { get; set; } = default!;
}
