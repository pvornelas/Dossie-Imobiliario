namespace DossieImobiliario.Dtos;

public record class CriarProcessoRequest
{
    public string Cliente { get; set; } = default!;
    public string Imovel { get; set; } = default!;
    public List<string> TiposDocumento { get; set; } = default!;
    public List<IFormFile> Arquivos { get; set; } = default!;
}

public record ProcessoListaItemResponse(
    int Id,
    string NumeroProcesso,
    string Cliente,
    string Imovel,
    DateTime CriadoEmUtc,
    string DownloadUrl
);

public record ProcessoResponse(
    int Id,
    string NumeroProcesso,
    string Cliente,
    string Imovel,
    DateTime CriadoEmUtc,
    List<DocumentoResponse> Documentos,
    string DownloadUrl
);

public record DocumentoResponse(
    int Id,
    string TipoDocumento,
    string NomeOriginal,
    string ContentType,
    long TamanhoBytes,
    DateTime EnviadoEmUtc
);