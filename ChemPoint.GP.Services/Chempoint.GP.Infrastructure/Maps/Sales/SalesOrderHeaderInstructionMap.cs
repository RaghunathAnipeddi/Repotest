using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderHeaderInstructionMap : BaseDataTableMap<SalesInstruction>
    {
        public override SalesInstruction Map(DataRow dr)
        {
            SalesInstruction salesHeaderInstruction = new SalesInstruction();
           
            int value;
            int.TryParse(dr["SopType"].ToString(), out value);
            salesHeaderInstruction.SopType = value;
            salesHeaderInstruction.SopNumber = dr["SopNumber"].ToString();
            salesHeaderInstruction.InstructionCode = dr["HeaderInstructionCode"].ToString();
            int.TryParse(dr["InstructionTypeID"].ToString(), out value);
            salesHeaderInstruction.InstructionTypeID = value;
            salesHeaderInstruction.CommentText = dr["CommentText"].ToString();
            return salesHeaderInstruction;
        }
    }
}
