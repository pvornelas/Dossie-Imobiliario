namespace DossieImobiliario.Data.Storage;
public class LocalFileStorage
{
    private readonly IWebHostEnvironment _env;
    private readonly string _diretorioBaseRelativo;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

    public LocalFileStorage(IWebHostEnvironment env, IConfiguration configuration)
    {
        _env = env;
        _diretorioBaseRelativo = configuration["Storage:Documentos"] ?? throw new InvalidOperationException("Storage:Documentos não configurado.");
    }

    private string DiretorioBaseAbsoluto =>
        Path.IsPathRooted(_diretorioBaseRelativo)
            ? _diretorioBaseRelativo
            : Path.Combine(_env.ContentRootPath, _diretorioBaseRelativo);

    private string DiretorioDoProcesso(string pastaProcesso) =>
        Path.Combine(DiretorioBaseAbsoluto, pastaProcesso);

    private static string NomeSeguro(string nome) => Path.GetFileName(nome);

    public async Task<List<ArquivoSalvoResult>> SalvarTodosAsync(string pastaProcesso, List<(string TipoDocumento, IFormFile Arquivo)> arquivos, CancellationToken ct = default)
    {
        if (arquivos is null || arquivos.Count == 0)
            return new List<ArquivoSalvoResult>();

        var pasta = DiretorioDoProcesso(pastaProcesso);
        Directory.CreateDirectory(pasta);

        var resultados = new List<ArquivoSalvoResult>();

        foreach (var (tipo, arquivo) in arquivos)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new InvalidOperationException("TipoDocumento inválido.");

            if (arquivo is null || arquivo.Length == 0)
                throw new InvalidOperationException("Arquivo inválido.");

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            var nomeSalvo = $"{Guid.NewGuid():N}{extensao}";
            var caminhoCompleto = Path.Combine(pasta, nomeSalvo);

            using (var fs = new FileStream(caminhoCompleto, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await arquivo.CopyToAsync(fs, ct);
            }

            var caminhoRelativo = $"{pastaProcesso}/{nomeSalvo}";

            resultados.Add(new ArquivoSalvoResult(
                TipoDocumento: tipo.Trim(),
                NomeOriginal: NomeSeguro(arquivo.FileName),
                NomeSalvo: nomeSalvo,
                CaminhoRelativo: caminhoRelativo,
                ContentType: string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
                TamanhoBytes: arquivo.Length
            ));
        }

        return resultados;
    }

    public async Task<DownloadResult> DownloadTodosAsync(List<(string NomeOriginal, string CaminhoRelativo)> arquivos, string nomeZip, CancellationToken ct = default)
    {
        if (arquivos is null || arquivos.Count == 0)
            throw new InvalidOperationException("Nenhum arquivo informado.");

        if (arquivos.Count == 1)
        {
            var (nomeOriginal, caminhoRelativo) = arquivos[0];

            var caminhoCompleto = ObterCaminhoCompleto(caminhoRelativo);
            if (!File.Exists(caminhoCompleto))
                throw new FileNotFoundException("Arquivo não encontrado.");

            if (!_contentTypeProvider.TryGetContentType(caminhoCompleto, out var contentType))
                contentType = "application/octet-stream";

            Stream stream = new FileStream(caminhoCompleto, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

            return DownloadResult.Arquivo(stream, contentType, NomeSeguro(nomeOriginal));
        }

        using (var ms = new MemoryStream())
        {
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (nomeOriginal, caminhoRelativo) in arquivos)
                {
                    var caminhoCompleto = ObterCaminhoCompleto(caminhoRelativo);
                    if (!File.Exists(caminhoCompleto))
                        throw new FileNotFoundException($"Arquivo não encontrado: {nomeOriginal}");

                    var entryName = NomeSeguro(nomeOriginal);
                    var entry = zip.CreateEntry(entryName, CompressionLevel.Fastest);

                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(caminhoCompleto, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                    {
                        await fileStream.CopyToAsync(entryStream, ct);
                    }
                }
            }

            return DownloadResult.Zip(ms.ToArray(), nomeZip);
        }
    }

    public string ObterCaminhoCompleto(string caminhoRelativo)
    {
        var caminhoSeguro = caminhoRelativo.Replace('\\', '/').TrimStart('/');
        return Path.Combine(DiretorioBaseAbsoluto, caminhoSeguro.Replace('/', Path.DirectorySeparatorChar));
    }

    public void ExcluirArquivo(string caminhoRelativo)
    {
        var caminhoCompleto = ObterCaminhoCompleto(caminhoRelativo);
        if (File.Exists(caminhoCompleto))
            File.Delete(caminhoCompleto);
    }

    public void RemoverPastaDoProcesso(string pastaProcesso)
    {
        var pasta = DiretorioDoProcesso(pastaProcesso);
        if (Directory.Exists(pasta))
            Directory.Delete(pasta, recursive: true);
    }

    public record ArquivoSalvoResult(
        string TipoDocumento,
        string NomeOriginal,
        string NomeSalvo,
        string CaminhoRelativo,
        string ContentType,
        long TamanhoBytes
    );

    public abstract record DownloadResult
    {
        public sealed record ArquivoResult(Stream Stream, string ContentType, string NomeDownload) : DownloadResult;
        public sealed record ZipResult(byte[] Bytes, string NomeDownload) : DownloadResult;
        public static DownloadResult Arquivo(Stream s, string ct, string nome) => new ArquivoResult(s, ct, nome);
        public static DownloadResult Zip(byte[] bytes, string nome) => new ZipResult(bytes, nome);
    }
}