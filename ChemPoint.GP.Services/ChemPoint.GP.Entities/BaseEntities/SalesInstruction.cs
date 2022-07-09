using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class SalesInstruction : IModelBase
    {
        public int OrderInstructionId { get; set; }

        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public string InstructionCode { get; set; }

        public int InstructionTypeID { get; set; }

        public string CommentText { get; set; }

        public int LineItemSequence { get; set; }

        public int ComponentSequenceNumber { get; set; }
    }
}
