using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut
{
    public class PayableDetailsEntity
    {
        private Collection<PayableLineEntity> _lineDetails;
        public PayableDetailsEntity()
        {
            _lineDetails = new Collection<PayableLineEntity>();
        }
        public Collection<PayableLineEntity> GetInvoiceLineDetails { get { return _lineDetails; } }
        public void AddInvoiceLine(PayableLineEntity invLine)
        {
            _lineDetails.Add(invLine);
        }
        //public DataTable TaxDetails;
        //public DataTable DistributedDetails;
    }
}
