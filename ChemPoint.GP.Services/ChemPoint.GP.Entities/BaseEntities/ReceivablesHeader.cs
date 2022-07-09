using System;
using System.Collections.Generic;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class ReceivablesHeader : IModelBase
    {
        public int DocumentType { get; set; }

        public string DocumentNumber { get; set; }

        public CustomerInformation CustomerInformation { get; set; }

        public string DocumentStatus { get; set; }

        public Amount Amount { get; set; }

        public int TypeId { get; set; }

        public string CommentText { get; set; }
    }
}
