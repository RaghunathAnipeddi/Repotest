using System.Data;

namespace Chempoint.GP.Infrastructure.DataAccessEngine.Commands
{
    public class StoredProcCommand : CommandBase
    {
        public StoredProcCommand() : base()
        {
        }

        public StoredProcCommand(string commandText) : base()
        {
            this.CommandText = commandText;
            this.TimeOut = 10000;
        }

        public override CommandType CommandType
        {
            get
            {
                return CommandType.StoredProcedure;
            }
        }
    }
}
