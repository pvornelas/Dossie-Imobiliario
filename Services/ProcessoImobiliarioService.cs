namespace DossieImobiliario.Services;

public class ProcessoImobiliarioService
{
    private readonly AppDbContext _db;
    private readonly LocalFileStorage _storage;

    public ProcessoImobiliarioService(AppDbContext db, LocalFileStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<(ProcessoImobiliario Processo, List<DocumentoProcesso> Documentos)> CriarAsync(
        string cliente,
        string imovel,
        List<(string TipoDocumento, IFormFile Arquivo)> documentos,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new InvalidOperationException("Cliente inválido.");

        if (string.IsNullOrWhiteSpace(imovel))
            throw new InvalidOperationException("Imóvel inválido.");

        var processo = new ProcessoImobiliario
        {
            Cliente = cliente.Trim(),
            Imovel = imovel.Trim(),
            NumeroProcesso = "PENDENTE",
            CriadoEm = DateTime.Now
        };

        _db.Processos.Add(processo);
        await _db.SaveChangesAsync(ct);

        processo.NumeroProcesso = GerarNumeroProcesso(processo.Id);
        await _db.SaveChangesAsync(ct);

        if (documentos is null || documentos.Count == 0)
            return (processo, new List<DocumentoProcesso>());

        List<ArquivoSalvoResult> salvos = new();
        try
        {
            salvos = await _storage.SalvarTodosAsync(processo.NumeroProcesso, documentos, ct);

            var docs = salvos.Select(s => new DocumentoProcesso
            {
                ProcessoImobiliarioId = processo.Id,
                TipoDocumento = s.TipoDocumento,
                NomeOriginal = s.NomeOriginal,
                NomeSalvo = s.NomeSalvo,
                CaminhoRelativo = s.CaminhoRelativo,
                ContentType = s.ContentType,
                TamanhoBytes = s.TamanhoBytes,
                EnviadoEm = DateTime.Now
            }).ToList();

            _db.Documentos.AddRange(docs);
            await _db.SaveChangesAsync(ct);

            return (processo, docs);
        }
        catch
        {
            foreach (var s in salvos)
                _storage.ExcluirArquivo(s.CaminhoRelativo);

            _storage.RemoverPastaDoProcesso(processo.NumeroProcesso);


            var proc = await _db.Processos.FirstOrDefaultAsync(p => p.Id == processo.Id, ct);
            if (proc is not null)
            {
                _db.Processos.Remove(proc);
                await _db.SaveChangesAsync(ct);
            }

            throw;
        }
    }

    public async Task<List<ProcessoImobiliario>> ListarPorClienteAsync(string? cliente, CancellationToken ct = default)
    {
        var query = _db.Processos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            var termo = cliente.Trim();
            query = query.Where(p => p.Cliente.Contains(termo));
        }

        return await query
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync(ct);
    }

    public async Task<ProcessoImobiliario?> BuscarPorNumeroAsync(string numeroProcesso, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numeroProcesso))
            return null;

        var numero = numeroProcesso.Trim();

        return await _db.Processos
            .AsNoTracking()
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.NumeroProcesso == numero, ct);
    }

    public async Task<DownloadResult?> DownloadDocumentosDoProcessoAsync(
        string numeroProcesso,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numeroProcesso))
            return null;

        var numero = numeroProcesso.Trim();

        var docs = await _db.Documentos
            .AsNoTracking()
            .Where(d => d.Processo!.NumeroProcesso == numero)
            .Select(d => new { d.NomeOriginal, d.CaminhoRelativo })
            .ToListAsync(ct);

        if (docs.Count == 0)
            return null;

        var lista = docs
            .Select(d => (d.NomeOriginal, d.CaminhoRelativo))
            .ToList();

        var nomeZip = $"{numero}_documentos.zip";

        return await _storage.DownloadTodosAsync(lista, nomeZip, ct);
    }

    public async Task<bool> RemoverPorNumeroAsync(string numeroProcesso, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(numeroProcesso))
            return false;

        var numero = numeroProcesso.Trim();

        var processo = await _db.Processos
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.NumeroProcesso == numero, ct);

        if (processo is null)
            return false;

        _db.Processos.Remove(processo);
        await _db.SaveChangesAsync(ct);

        _storage.RemoverPastaDoProcesso(numero);

        return true;
    }

    private static string GerarNumeroProcesso(int id) => $"IMOB-{DateTime.Now:yyyy}-{id:D6}";
}