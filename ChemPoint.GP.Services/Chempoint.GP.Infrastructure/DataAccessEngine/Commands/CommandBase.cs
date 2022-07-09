using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider;
using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.Commands
{
    public abstract class CommandBase : ICommand
    {
        protected CommandBase()
        {
            Parameters = new SqlDbParameterCollection();
        }

        public abstract CommandType CommandType { get; }

        public SqlDbParameterCollection Parameters { get; set; }

        public string CommandText { get; set; }

        public int TimeOut { get; set; }
    }
}
