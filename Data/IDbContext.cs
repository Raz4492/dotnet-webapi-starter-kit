using Microsoft.Data.SqlClient;

namespace SmartAPI.Data
{
    public interface IDbContext
    {
        SqlConnection CreateConnection();
        Task<SqlConnection> CreateConnectionAsync();
    }
}