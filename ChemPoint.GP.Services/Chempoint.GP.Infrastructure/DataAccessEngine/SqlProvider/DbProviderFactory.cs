using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.SqlClient;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider
{
    public static class DbProviderFactory
    {
        public static SqlDatabase CreateSqlDataBase(string connectionString)
        {
            return new SqlDatabase(connectionString);
        }

        public static SqlDatabase CreateSqlDataBase(SqlConnection sqlConn)
        {
            return new SqlDatabase(sqlConn.ConnectionString.ToString());
        }
    }
}
