using System.Data.SqlClient;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context
{
    public class GpAddInDbContext : DbContext
    {
        public GpAddInDbContext(string connectionString) : base(connectionString)
        {
        }

        public GpAddInDbContext(SqlConnection sqlConn) : base(sqlConn)
        {
        }
    }
}
