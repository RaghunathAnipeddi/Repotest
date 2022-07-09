using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Chempoint.GP.Infrastructure.Extensions;
using ChemPoint.GP.Entities.BaseEntities;
using Chempoint.GP.Infrastructure.DataAccessEngine.Commands;
using Chempoint.GP.Infrastructure.Maps.Base;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider;
using Chempoint.GP.Infrastructure.DataAccessEngine.Extensions;
using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider.Context;

namespace Chempoint.GP.Infrastructure.Utils
{
    public abstract class RepositoryBase
    {
        private IDbContext DbContext { get; set; }

        protected RepositoryBase(IDbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        protected virtual object Insert(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteScalar(cmd);
            }
        }

        protected virtual object InsertReturn(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                var result = DbContext.ExecuteScalar(cmd);
                return cmd;
            }
        }

        protected virtual int Delete(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteNonQuery(cmd);
            }
        }

        protected virtual int Update(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteNonQuery(cmd);
            }
        }

        protected virtual TModel Get<TModel, TMap>(ICommand command)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRecordMap<TModel>, new()
        {
            using (var cmd = GetSqlCommand(command))
            {
                IDataReader reader = DbContext.ExecuteReader(cmd);

                using (reader)
                {
                    return reader.Select(MapperFactory<TModel, TMap>.Mapper().Map).FirstOrDefault();
                }
            }
        }

        protected virtual object GetSingle(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteScalar(cmd);
            }
        }

        protected virtual IDataReader GetDataReader(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteReader(cmd);
            }
        }

        protected virtual IEnumerable<TModel> FindAll<TModel, TMap>(ICommand command)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRecordMap<TModel>, new()
        {
            using (var cmd = GetSqlCommand(command))
            {
                IDataReader reader = DbContext.ExecuteReader(cmd);

                using (reader)
                {
                    return reader.Select(MapperFactory<TModel, TMap>.Mapper().Map).ToList();
                }
            }
        }

        protected IEnumerable<TModel> GetAllEntities<TModel, TMap>(DataTable dt)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRowMap<TModel>, new()
        {
            var lst = new List<TModel>();

            foreach (DataRow dataRow in dt.Rows)
                lst.Add(dataRow.SelectRow(MapperFactory<TModel, TMap>.Mapper().Map));
            return lst;
        }

        /// <summary>
        /// Mapping the value to Different base entity
        /// </summary>
        /// <typeparam name="CashApplicationRequest"></typeparam>
        /// <typeparam name="TMap"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        protected List<TModel> GetAllDifferentEntities<TModel, TMap>(DataTable dt)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRowMap<TModel>, new()
        {
            var lst = new List<TModel>();

            foreach (DataRow dataRow in dt.Rows)
                lst.Add(dataRow.SelectRow(MapperFactory<TModel, TMap>.Mapper().Map));
            return lst;
        }

        protected TModel GetEntity<TModel, TMap>(DataTable dt)
            where TModel : class, IModelBase, new()
            where TMap : class, IDataRowMap<TModel>, new()
        {
            if (dt.Rows.Count > 0)
                return dt.Rows[0].SelectRow(MapperFactory<TModel, TMap>.Mapper().Map);
            else
                return default(TModel);
        }

        protected virtual DataSet GetDataSet(ICommand command)
        {
            using (var cmd = GetSqlCommand(command))
            {
                return DbContext.ExecuteDataSet(cmd);
            }
        }

        private SqlCommand GetSqlCommand(ICommand command)
        {
            SqlCommand sqlCommand = null;

            switch (command.CommandType)
            {
                case CommandType.StoredProcedure:
                    sqlCommand = DbContext.GetStoredProcCommand(command.CommandText) as SqlCommand;
                    sqlCommand.CommandTimeout = 10000;
                    break;
                case CommandType.Text:
                    sqlCommand = DbContext.GetSqlStringCommand(command.CommandText) as SqlCommand;
                    break;
            }

            if (command.Parameters.ParameterCollection.Count > 0)
                AddParamaters(sqlCommand, command.Parameters);

            return sqlCommand;
        }

        private void AddParamaters(SqlCommand sqlCommand, SqlDbParameterCollection sqlDbParametersCollection)
        {
            var sqlDbParameters = sqlDbParametersCollection.ParameterCollection.Values.ToList();
            List<SqlParameter> sqlParameters = new List<SqlParameter>(sqlDbParameters.Count);

            foreach (var param in sqlDbParameters)
            {
                var sqlParameter = new SqlParameter(string.Format("@{0}", param.Name), param.DbType);

                if (param.Size.HasValue)
                    sqlParameter.Size = param.Size.Value;

                switch (param.Direction)
                {
                    case ParamDirection.Input:
                        sqlParameter.Value = param.Value;

                        sqlParameter.Direction = ParameterDirection.Input;

                        if (param.TypeName.IsValid())
                            sqlParameter.TypeName = param.TypeName;
                        break;
                    case ParamDirection.Outpt:
                        sqlParameter.Direction = ParameterDirection.Output;
                        break;
                }

                sqlParameters.Add(sqlParameter);
            }

            sqlCommand.Parameters.AddRange(sqlParameters.ToArray());
        }

        protected SqlStringCommand CreateSqlStringCommand()
        {
            return new SqlStringCommand();
        }

        protected SqlStringCommand CreateSqlStringCommand(string commandText)
        {
            return new SqlStringCommand(commandText);
        }

        protected StoredProcCommand CreateStoredProcCommand(string commandText)
        {
            return new StoredProcCommand(commandText);
        }

        protected Guid GetPrimaryKeyFromId(object id)
        {
            id.ThrowIfNull("PrimaryKey is null");

            Guid result = Guid.Empty;
            Guid.TryParse(id.ToString(), out result);
            return result;
        }
    }
}
