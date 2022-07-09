using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class SalesOrderLineInstructionMap : BaseDataTableMap<SalesInstruction>
    {
        public override SalesInstruction Map(DataRow dr)
        {
            var sopOrderInstructionDetail = new SalesInstruction();
            int value;
            int.TryParse(dr["SopType"].ToString(), out value);
            sopOrderInstructionDetail.SopType = value;
            sopOrderInstructionDetail.SopNumber = dr["SopNumber"].ToString();
            sopOrderInstructionDetail.InstructionCode = dr["LineInstructionCode"].ToString();
            int.TryParse(dr["InstructionTypeId"].ToString(), out value);
            sopOrderInstructionDetail.InstructionTypeID = value;
            sopOrderInstructionDetail.CommentText = dr["CommentText"].ToString();
            int.TryParse(dr["LineItemSequence"].ToString(), out value);
            sopOrderInstructionDetail.LineItemSequence = value;
            int.TryParse(dr["ComponentLineSeqNumber"].ToString(), out value);
            sopOrderInstructionDetail.ComponentSequenceNumber = value;
            return sopOrderInstructionDetail;
        }
    }
}
