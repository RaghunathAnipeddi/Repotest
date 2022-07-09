using System.Data;
using System.Data.SqlClient;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context
{
    public interface IDbContext
    {
        IDbCommand GetSqlStringCommand(string query);

        IDbCommand GetStoredProcCommand(string storedProcedureName);

        DataSet ExecuteDataSet(SqlCommand sqlCommand);

        int ExecuteNonQuery(SqlCommand sqlCommand);

        object ExecuteScalar(SqlCommand sqlCommand);

        IDataReader ExecuteReader(SqlCommand sqlCommand);
    }
}
