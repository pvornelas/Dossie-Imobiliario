using Dapper;
using System.Data;

namespace DossieImobiliario.Data.Repository;

public class ProcessoImobiliarioRepository
{
    private readonly string _connStr;

    public ProcessoImobiliarioRepository(IConfiguration configuration)
    {
        _connStr = configuration.GetConnectionString("Database")!;
    }

    private IDbConnection Connection() => new SqliteConnection(_connStr);

    public async Task<IEnumerable<ProcessoImobiliario>> ListarProcessos()
    {
        using var conn = Connection();

        var sql = "SELECT * FROM ProcessoImobiliario";

        return await conn.QueryAsync<ProcessoImobiliario>(sql);
    }

    public async Task<IEnumerable<ProcessoImobiliario>> ListarProcessosPorNome(string nome)
    {
        using var conn = Connection();

        var sql = "SELECT * FROM ProcessoImobiliario WHERE Cliente = %@Cliente%";

        return await conn.QueryAsync<ProcessoImobiliario>(sql);
    }
}
