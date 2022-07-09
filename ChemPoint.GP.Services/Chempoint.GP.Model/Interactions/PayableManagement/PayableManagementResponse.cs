using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.PayableManagement
{
    /// <summary>
    /// enum for responce status.
    /// </summary>
    public enum Response
    { Success = 0, Error = 1, Unknown = 2 }

    public class PayableManagementResponse
    {
        private StringBuilder _logFileMessage;

        public PayableManagementResponse()
        {
            _logFileMessage = new StringBuilder();
        }

        private Response _status;
        private string _message;
        private string _fileName;
        private PayableDetailsEntity _invoiceDetails;
        private PayableLookupDetailEntity _lookupDetails;
        private Boolean _lookupValueExists;
        private Boolean _isValid;
        private int _decimalPlaces;
        private string _userId;
        private Int16 _isValidStatus;

        public string UserId { get { return _userId; } set { _userId = value; } }
        public int CurrencyDecimal { get { return _decimalPlaces; } set { _decimalPlaces = value; } }
        public Boolean IsValid { get { return _isValid; } set { _isValid = value; } }
        public Response Status { get { return _status; } set { _status = value; } }
        public StringBuilder LogFileMessage { get { return _logFileMessage; } set { _logFileMessage = value; } }
        public string FileName { get { return _fileName; } set { _fileName = value; } }
        public PayableDetailsEntity InvoiceDetail { get { return _invoiceDetails; } set { _invoiceDetails = value; } }
        public PayableLookupDetailEntity LookupDetail { get { return _lookupDetails; } set { _lookupDetails = value; } }
        public Boolean LookupValueExists { get { return _lookupValueExists; } set { _lookupValueExists = value; } }
        public string Message { get { return _message; } set { _message = value; } }
        public Int16 IsValidStatus { get { return _isValidStatus; } set { _isValidStatus = value; } }

        #region PayableService

        public string ErrorMessage { get; set; }
        //public int CurrencyDecimal { get; set; }
        public string LogMessage { get; set; }
       // public Boolean IsValid { get; set; }
        //public int IsValidStatus { get; set; }
        //public Boolean LookupValueExists { get; set; }
        public string ApiDistributionDtValue { get; set; }
        public PayableLookupDetailEntity LookupDetails { get; set; }
        public PayableDetailsEntity PayableDetailsEntity { get; set; }
        public List<PayableLineEntity> DuplicationValidationList { get; set; }
        public List<PayableLineEntity> CTSIValidationList { get; set; }
        #endregion
    }
}
