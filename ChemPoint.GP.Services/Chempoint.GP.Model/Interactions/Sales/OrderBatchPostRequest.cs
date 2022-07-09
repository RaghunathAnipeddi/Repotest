using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.Entities.Business_Entities.Sales;
using System.Collections.ObjectModel;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public enum OrderPostOperationType
    {
        None = 0,
        BatchValidate = 1,
        RemoveLockedUsers = 2,
        IntercompanySalesTransactions = 3,
        CurrencyIssues = 4,
        VatIssues = 5,
        CorrectDropships = 6,
        ProcessCreditCards = 7,
        DistributionIssues = 8,
        PostTransactions = 9,
        UnlockPostingBatch = 10,
        LockPostingBatch = 11,
        SsrsReport = 12,
        CadTaxExemptions = 13,
        MissingShipVia = 14,     /// HP 97272
        CanadianTaxIssue = 15,   /// HP 113437 - CAD Avatax Issue Fix 
        ServiceSkuIssue = 16,    /// Service Sku Automation
        DropShipIssues = 17,     /// Drop Ship Issue
        LinkedPayment = 18,      /// LinkedPayment Issue
        FailedPrepayment = 19   /// FailedPrepayment Issue
        
    }

    public class OrderBatchPostRequest
    {
        public OrderBatchPostRequest()
        {
            lockedUsers = new Collection<User>();
        }

        public SendEmailRequest EmailRequest { get; set; }

        public int CompanyID { get; set; }

        public string CompanyName { get; set; }

        public string ConnectionString { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string BatchNumber { get; set; }

        public OrderPostOperationType OperationType { get; set; }

        public SalesOrderPostEntity BatchDetails { get; set; }

        private Collection<User> lockedUsers;

        public Collection<User> LockedUsers
        {
            get
            {
                return lockedUsers;
            }
        }

        public string OpenOrdersSsrsReportUrl { get; set; }

        
        
        public void AddLockedUserToBatch(string userId, string userName, int companyId)
        {
            lockedUsers.Add(new User() { UserId = userId, UserName = userName, CompanyId = companyId });
        }
    }
}
