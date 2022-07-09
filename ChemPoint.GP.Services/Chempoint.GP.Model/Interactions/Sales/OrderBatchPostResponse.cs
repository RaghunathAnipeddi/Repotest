using ChemPoint.Entities.Business_Entities.Sales;
using ChemPoint.GP.Entities.Business_Entities.Sales;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class OrderBatchPostResponse
    {
        public OrderBatchPostResponse()
        {
            usersHoldingBatch = new Collection<User>();
        }

        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }

        public SalesOrderPostEntity BatchDetails { get; set; }

        private Collection<User> usersHoldingBatch;

        public Collection<User> UsersHoldingBatch
        {
            get
            {
                return usersHoldingBatch;
            }
        }

        public bool IsBatchLockCleared { get; set; }

        public bool IsUsdccTransactionsAvailable { get; set; }

        public bool IsAllSsrsFileOpened { get; set; }

        public string SsrsReportLink { get; set; }

        public string EmailContent { get; set; }
        

        public void AddUserToHoldingBatch(string userId, string userName, int companyId)
        {
            usersHoldingBatch.Add(new User() { UserId = userId, UserName = userName, CompanyId = companyId });
        }
    }
}
