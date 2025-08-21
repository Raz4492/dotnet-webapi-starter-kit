using Microsoft.Data.SqlClient;
using SmartAPI.Data;

namespace SmartAPI.Data
{
    public class DapperContext : IDbContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}