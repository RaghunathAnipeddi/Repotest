using System.Collections.Generic;
using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider
{
    public class SqlDbParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public SqlDbType DbType { get; set; }

        public int? Size { get; set; }

        public string TypeName { get; set; }

        public ParamDirection Direction { get; private set; }

        public SqlDbParameter(string name, SqlDbType dbType, ParamDirection direction, int? size)
        {
            this.Name = name;
            this.DbType = dbType;
            this.Size = size;
            this.Direction = direction;
        }

        public SqlDbParameter(string name, SqlDbType dbType, ParamDirection direction, object value, int? size = null, string typeName = null)
        {
            this.Name = name;
            this.DbType = dbType;
            this.Direction = direction;
            this.Value = value;
            this.Size = size;
            this.TypeName = typeName;
        }
    }

    public class SqlDbParameterCollection
    {
        public Dictionary<string, SqlDbParameter> ParameterCollection { get; private set; }

        public SqlDbParameterCollection()
        {
            ParameterCollection = new Dictionary<string, SqlDbParameter>();
        }

        public void AddInputParams(string name, SqlDbType dbType, object value, int? size = null, string typeName = null)
        {
            ParameterCollection.Add(name, new SqlDbParameter(name, dbType, ParamDirection.Input, value, size, typeName));
        }

        public void AddOutputParams(string name, SqlDbType dbType, int? size = null)
        {
            ParameterCollection.Add(name, new SqlDbParameter(name, dbType, ParamDirection.Outpt, size));
        }
    }

    public enum ParamDirection : short
    {
        Input = 0,
        Outpt = 1
    }
}
