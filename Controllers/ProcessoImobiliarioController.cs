namespace DossieImobiliario.Controllers;
[ApiController]
[Route("api/processo-imobiliario")]
public class ProcessoImobiliarioController : ControllerBase
{
    private readonly ProcessoImobiliarioService _service;
    private readonly ProcessoImobiliarioRepository _repo;

    public ProcessoImobiliarioController(ProcessoImobiliarioService service, ProcessoImobiliarioRepository repo)
    {
        _service = service;
        _repo = repo;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Criar([FromForm] CriarProcessoRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Cliente) || string.IsNullOrWhiteSpace(request.Imovel))
            return BadRequest("Cliente e Imóvel são obrigatórios.");

        request.TiposDocumento ??= new();
        request.Arquivos ??= new();

        if (request.Arquivos.Count > 0 && request.TiposDocumento.Count != request.Arquivos.Count)
            return BadRequest("TiposDocumento e Aquivos devem ter a mesma quantidade.");

        var docs = new List<(string TipoDocumento, IFormFile Arquivo)>();
        for (int i = 0; i < request.Arquivos.Count; i++)
            docs.Add((request.TiposDocumento[i], request.Arquivos[i]));

        var (processo, documentos) = await _service.CriarAsync(request.Cliente, request.Imovel, docs, ct);

        var downloadUrl = $"/api/processo-imobiliario/{processo.NumeroProcesso}/download";

        var response = new ProcessoResponse(
            Id: processo.Id,
            NumeroProcesso: processo.NumeroProcesso,
            Cliente: processo.Cliente,
            Imovel: processo.Imovel,
            CriadoEmUtc: processo.CriadoEm,
            Documentos: documentos.Select(d => new DocumentoResponse(
                Id: d.Id,
                TipoDocumento: d.TipoDocumento,
                NomeOriginal: d.NomeOriginal,
                ContentType: d.ContentType,
                TamanhoBytes: d.TamanhoBytes,
                EnviadoEmUtc: d.EnviadoEm
            )).ToList(),
            DownloadUrl: downloadUrl
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? cliente, CancellationToken ct)
    {
        var processos = await _service.ListarPorClienteAsync(cliente, ct);

        var response = processos.Select(p => new ProcessoListaItemResponse(
            Id: p.Id,
            NumeroProcesso: p.NumeroProcesso,
            Cliente: p.Cliente,
            Imovel: p.Imovel,
            CriadoEmUtc: p.CriadoEm,
            DownloadUrl: $"/api/processo-imobiliario/{p.NumeroProcesso}/download"
        )).ToList();

        return Ok(response);
    }

    [HttpGet("{numeroProcesso}")]
    public async Task<IActionResult> BuscarPorNumero(string numeroProcesso, CancellationToken ct)
    {
        var processo = await _service.BuscarPorNumeroAsync(numeroProcesso, ct);
        if (processo is null)
            return NotFound("Processo não encontrado.");

        var downloadUrl = $"/api/processo-imobiliario/{processo.NumeroProcesso}/download";

        var response = new ProcessoResponse(
            Id: processo.Id,
            NumeroProcesso: processo.NumeroProcesso,
            Cliente: processo.Cliente,
            Imovel: processo.Imovel,
            CriadoEmUtc: processo.CriadoEm,
            Documentos: processo.Documentos
                .OrderByDescending(d => d.EnviadoEm)
                .Select(d => new DocumentoResponse(
                    Id: d.Id,
                    TipoDocumento: d.TipoDocumento,
                    NomeOriginal: d.NomeOriginal,
                    ContentType: d.ContentType,
                    TamanhoBytes: d.TamanhoBytes,
                    EnviadoEmUtc: d.EnviadoEm
                )).ToList(),
            DownloadUrl: downloadUrl
        );

        return Ok(response);
    }

    [HttpGet("{numeroProcesso}/download")]
    public async Task<IActionResult> Download(string numeroProcesso, CancellationToken ct)
    {
        var result = await _service.DownloadDocumentosDoProcessoAsync(numeroProcesso, ct);
        if (result is null)
            return NotFound("Nenhum documento encontrado para este processo.");

        return result switch
        {
            LocalFileStorage.DownloadResult.ArquivoResult arq =>
                File(arq.Stream, arq.ContentType, arq.NomeDownload),

            LocalFileStorage.DownloadResult.ZipResult zip =>
                File(zip.Bytes, "application/zip", zip.NomeDownload),

            _ => BadRequest("Formato de download não suportado.")
        };
    }

    [HttpDelete("{numeroProcesso}")]
    public async Task<IActionResult> Remover(string numeroProcesso, CancellationToken ct)
    {
        var ok = await _service.RemoverPorNumeroAsync(numeroProcesso, ct);
        if (!ok)
            return NotFound("Processo não encontrado.");

        return Ok(new { mensagem = "Processo excluído com sucesso." });
    }
}