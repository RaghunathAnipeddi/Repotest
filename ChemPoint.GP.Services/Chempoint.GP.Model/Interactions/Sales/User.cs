using System;

namespace Chempoint.GP.Model.Interactions.Sales
{
    public class User
    {
        private string userId;
        private string userName;
        private int companyId;

        public string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                userId = value;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        public int CompanyId
        {
            get
            {
                return companyId;
            }
            set
            {
                companyId = value;
            }
        }
    }
}