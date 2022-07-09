using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.Commands
{
    public class SqlStringCommand : CommandBase
    {
        public SqlStringCommand() : base()
        {
        }

        public SqlStringCommand(string commandText) : base()
        {
            this.CommandText = commandText;
        }

        public override CommandType CommandType
        {
            get
            {
                return CommandType.Text;
            }
        }
    }
}
