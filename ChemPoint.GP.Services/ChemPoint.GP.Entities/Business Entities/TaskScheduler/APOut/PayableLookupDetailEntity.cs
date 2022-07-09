using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut
{
    public class PayableLookupDetailEntity
    {
        //private string _invoiceNumber;
        private Collection<PayableLookupLineEntity> _lookupDetails;

        public PayableLookupDetailEntity()
        {
            _lookupDetails = new Collection<PayableLookupLineEntity>();
        }

        public Collection<PayableLookupLineEntity> GetLookupDetails { get { return _lookupDetails; } }

        public void AddLookupLine(PayableLookupLineEntity lookupLine)
        {
            _lookupDetails.Add(lookupLine);
        }
    }
}
