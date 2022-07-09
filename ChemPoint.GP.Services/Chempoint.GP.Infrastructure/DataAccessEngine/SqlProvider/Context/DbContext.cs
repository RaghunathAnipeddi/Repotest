using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;
using System.Data.SqlClient;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context
{
    public abstract class DbContext : IDbContext
    {
        protected SqlDatabase sqlDatabase = null;

        public DbContext(string connectionString)
        {
            sqlDatabase = DbProviderFactory.CreateSqlDataBase(connectionString);
        }

        public DbContext(SqlConnection sqlConn)
        {
            // TODO: Complete member initialization
            sqlDatabase = DbProviderFactory.CreateSqlDataBase(sqlConn);
        }

        public virtual IDbCommand GetSqlStringCommand(string query)
        {
            return sqlDatabase.GetSqlStringCommand(query);
        }

        public virtual IDbCommand GetStoredProcCommand(string storedProcedureName)
        {
            return sqlDatabase.GetStoredProcCommand(storedProcedureName);
        }

        public virtual DataSet ExecuteDataSet(SqlCommand sqlCommand)
        {
            return sqlDatabase.ExecuteDataSet(sqlCommand);
        }

        public virtual int ExecuteNonQuery(SqlCommand sqlCommand)
        {
            return sqlDatabase.ExecuteNonQuery(sqlCommand);
        }

        public virtual object ExecuteScalar(SqlCommand sqlCommand)
        {
            return sqlDatabase.ExecuteScalar(sqlCommand);
        }

        public virtual IDataReader ExecuteReader(SqlCommand sqlCommand)
        {
            return sqlDatabase.ExecuteReader(sqlCommand);
        }
    }
}
