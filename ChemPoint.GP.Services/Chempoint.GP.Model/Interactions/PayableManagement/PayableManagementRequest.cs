using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Model.Interactions.PayableManagement
{
    public class PayableManagementRequest
    {
        public int companyId { get; set; }        
        public string UserId { get; set; }
        public string ConnectionString { get; set; }
        public string EconnectConString { get; set; }
        public int AppConfigID { get; set; }
        public PayableManagementEntity PayableManagementDetails { get; set; }
        public string PMInputHeader { get; set; }
        public string POInputHeader { get; set; }
        public string CHMPTFileNameForPO { get; set; }
        public string CPEURFileNameForPO { get; set; }
        public string CHMPTFileNameForNonPO { get; set; }
        public string CPEURFileNameForNonPO { get; set; }
        public string ExtractedFilesArchiveFolder { get; set; }
        public string CHMPTFileName { get; set; }
        public string CPEURFileName { get; set; }
        public string GBPFileName { get; set; }

        #region PayableService

       // public int companyId { get; set; }
        public string userId { get; set; }
       // public string ConnectionString { get; set; }
        public string FileName { get; set; }
        public string sourceFileName { get; set; }
        public string SourceFormName { get; set; }
        public string Source { get; set; }
        public string ManualPaymentNumber { get; set; }
        public string FromVendorId { get; set; }
        public string ToVendorId { get; set; }
        public string SearchType { get; set; }
        public int FromExpenseId { get; set; }
        public int ToExpenseId { get; set; }
        public DateTime FromDocumentDate { get; set; }
        public DateTime ToDocumentDate { get; set; }
        public string SourceLookup { get; set; }
        public string LookupValue { get; set; }
        //CTSI
        public string FromCtsiId { get; set; }
        public string ToCtsiId { get; set; }
        public string Company { get; set; }
        //API
        public string FromApiId { get; set; }
        public string ToApiId { get; set; }
        public int InvoiceType { get; set; }
//        public string ApiTransactionDtValue { get; set; }
  //      public string ApiDistributionDtValue { get; set; }
        //public PayableManagementEntity PayableManagementDetails { get; set; }
        public PayableLookupDetailEntity LookupDetails { get; set; }
        public PayableDetailsEntity PayableDetailsEntity { get; set; }

        //APIFileGenerator
        public string OutputFilePath { get; set; }
        public string VendorFileName { get; set; }
        public string LocFileName { get; set; }
        public string GlFileName { get; set; }
        public string PoFileName { get; set; }
        public string RcvFileName { get; set; }
        public string PayFileName { get; set; }
        public string VenGLFileName { get; set; }
        public string GPToAPITempFolderPath { get; set; }
        public string CutofMonth { get; set; }

        public string FTPInFolderPath { get; set; }
        public string FTPOutFolderPath { get; set; }
        public string ZipExtractedFilePath { get; set; }
        public string FTPPath { get; set; }
        public string FTPUserName { get; set; }
        public string FTPPassword { get; set; }
        public string FTPDownloadFile1 { get; set; }
        public string FTPDownloadFile2 { get; set; }
        public string TextFileExtension { get; set; }
        public int executionCount { get; set; }

        public string FileGenMailTo { get; set; }
        public string FileGenMailCc { get; set; }
        public string FileGenMailFrom { get; set; }
        public string FileGenMailGPToAPISubject { get; set; }
        public string FileGenSMTP { get; set; }
        public string FileGenMailBcc { get; set; }

        public string GPToAPILoggingPath { get; set; }
        public string GPToAPILogFileName { get; set; }

        public SendEmailRequest EmailRequest { get; set; }

        public List<PayableLineEntity> PoValidationList { get; set; }
        public List<PayableLineEntity> DuplicationValidationList { get; set; }
        //CTSI
        public List<PayableLineEntity> CTSIValidationList { get; set; }

        public List<PayableLineEntity> CTSITaxValidationList { get; set; }

        #endregion

        #region CTSIFileUploader

        public DateTime processingDate { get; set; }
        public string TempFolderPath { get; set; }
        public string InboundHeader { get; set; }
        public string OutboundHeader { get; set; }
        public string CustomerReturnHeader { get; set; }
        public string SupplierReturnHeader { get; set; }
        public string TransferHeader { get; set; }
        public string NaInboundFileName { get; set; }
        public string NaOutboundFileName { get; set; }
        public string NaTransfersFileName { get; set; }
        public string NaSupplierReturnFileName { get; set; }
        public string NaCustomerReturnFileName { get; set; }
        public string EuInboundFileName { get; set; }
        public string EuOutboundFileName { get; set; }
        public string EuCustomerReturnFileName { get; set; }
        public string OutputEuFileName { get; set; }
        public string OutputNaFileName { get; set; }
        public string DownloadForNAandEU { get; set; }
        public string FileNameStarts { get; set; }
        //CTSI Mail
        public string GPToCtsiMailSubject { get; set; }
        public string GPToCtsiMailFrom { get; set; }
        public string GPToCtsiMailCC { get; set; }
        public string GPToCtsiMailTo { get; set; }
        public string GPToCtsiMailBcc { get; set; }
        public string SMTPServcer { get; set; }
        public string CTSISignature { get; set; }
        public string CTSIUserId { get; set; }

        #endregion


    }
}
