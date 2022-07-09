using Chempoint.GP.Infrastructure.DataAccessEngine.SqlProvider;
using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.Commands
{
    public interface ICommand
    {
        CommandType CommandType { get; }

        SqlDbParameterCollection Parameters { get; set; }

        string CommandText { get; set; }
    }
}
