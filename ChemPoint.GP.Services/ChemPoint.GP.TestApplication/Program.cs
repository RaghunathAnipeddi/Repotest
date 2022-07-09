using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Chempoint.BusinessObjects;
using Chempoint.GP.Model.Interactions.Account;
using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            //#region Customer Information
            //bool isResult = false;
            //CustomerInformation customerInformation = new CustomerInformation();

            //Header header = new Header();
            //header.TransactionId = 22944;
            //header.OrginComponentId = 2;
            //header.SenderComponentID = 2;
            //header.DestinationComponentID = 3;
            //header.SourceEntity = "CustomerInformation";
            //customerInformation.Header = header;

            //Customer customer = new Customer();
            //customer.CustomerId = "60127439-4988-e411-80c6-0050587f2er3";
            //customer.CustomerNumber = "197";
            //customer.ParentCompanyId = "";

            //CreditProfile creditProfile = new CreditProfile();
            //creditProfile.CreditProfileId = "1470ec7a-1889-e511-80f9-0050569f7fa9";
            //creditProfile.CreditType = "";
            //creditProfile.PaymentTerms = "Net 30";

            //Amount amount = new Amount();
            //amount.Amount1 = 90000;
            //amount.CurrencyCode = CurrencyCode.USD;
            //creditProfile.CreditLimit = amount;

            //creditProfile.CorporateCreditType = "";
            //creditProfile.CreditHoldFlag = false;
            //creditProfile.CreditReviewRequiredFlag = true;
            //creditProfile.TerritoryExceptionFlag = false;
            //creditProfile.CustomerClass = "";
            //creditProfile.TaxCertStatus = "";
            //creditProfile.InheritCreditLimitfromParent = false;

            //CreditProfileTaxDetail creditProfileTaxDetail = new Chempoint.BusinessObjects.CreditProfileTaxDetail ();
            //creditProfileTaxDetail.CustomerDeclaration = "Exempt";
            //TaxInformation[] taxInformationArray = new TaxInformation[1];
            //TaxInformation taxInformation = new TaxInformation();
            //taxInformation.TaxId = "6791cca2-9703-e611-80cc-0050569f6243";
            //State taxState = new State();
            //taxState.StateID = "CA";
            //taxInformation.State = taxState;
            //Country taxCountry = new Country();
            //taxCountry.CountryID = "US";
            //taxInformation.Country = taxCountry;
            //taxInformation.VATNumber = "DE127470856";
            //taxInformation.IsValidtated = true;
            //Status taxStatus = new Status();
            //taxStatus.StatusCode = "0";
            //taxStatus.Description = "Active";
            //taxStatus.StatusReason = "Active";
            //taxInformation.Status = taxStatus;
            //taxInformationArray[0]= taxInformation;
            //creditProfileTaxDetail.TaxInformations = taxInformationArray;
            //creditProfile.TaxDetail = creditProfileTaxDetail;
            //customer.CreditProfile = creditProfile;

            //Status status = new Status();
            //status.StatusCode = "1";
            //status.Description = "Active";
            //status.StatusReason = "Active";
            //customer.Status = status;

            //AuditInformation auditInformation = new AuditInformation();
            //auditInformation.CreatedBy = "CHEMPOINT\\SQLAdminUser";
            //auditInformation.CreatedDate = "12/20/2014 1:08:08 PM";
            //auditInformation.ModifiedBy = "CHEMPOINT\\spalani";
            //auditInformation.ModifiedDate = "12/20/2014 1:08:08 PM";
            //customer.AuditInformation = auditInformation;

            //customer.CustomerStatus = "Active";
            //customer.Name = "Mechanical Rubber Products Co";
            //customer.Currency = CurrencyCode.USD;

            //Region region = new Region();
            //region.RegionID = "95928359-9164-e311-9b95-005056b00006";
            //region.Description = "North America";
            //customer.Region = region;

            //AddressInformation addressInformation = new Chempoint.BusinessObjects.AddressInformation();
            //addressInformation.AddressLine1 = "77 Forester Ave";
            //addressInformation.AddressLine2 = "";
            //addressInformation.City = "Warwick";
            //State state = new State();
            //state.StateID = "NY";
            //addressInformation.State = state;
            //addressInformation.ZipCode = "10990-1107";
            //Country country = new Country();
            //country.CountryID = "US";
            //addressInformation.Country = country;
            //customer.Address = addressInformation;

            //TelePhonesType telePhonesType = new TelePhonesType();
            //telePhonesType.MainPhone = "(221) 321-3213";
            //telePhonesType.MainPhoneExtension = "";
            //telePhonesType.MobilePhone = "";
            //telePhonesType.AlternatePhone = "";
            //telePhonesType.FaxNumber = "8459860399";
            //customer.PhoneNumber = telePhonesType;

            //customerInformation.CustomerAccount = customer;

            //AccountRequest accountRequest = new AccountRequest();
            //accountRequest.XrmCustomerInformation = customerInformation;

            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("http://localhost:63460/");
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    var response = client.PostAsJsonAsync("api/Customer/SaveAccountInGP/", accountRequest); // we need to refer the web.api service url here.
            //    if (response.Result.IsSuccessStatusCode)
            //    {
            //        AccountResponse accountResponse = response.Result.Content.ReadAsAsync<AccountResponse>().Result;
            //        if (accountResponse.Status == Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success)
            //        {
            //            isResult = true;
            //        }
            //        else
            //        {
            //            isResult = false;
            //        }
            //    }
            //    else
            //    {
            //        isResult = false;
            //    }
            //    if (!isResult)
            //    {

            //    }

            //    #endregion

            //#region Credit Profile
            //bool isResult = false;
            //CustomerInformation customerInformation = new CustomerInformation();

            //Header header = new Header();
            //header.TransactionId = 22944;
            //header.OrginComponentId = 2;
            //header.SenderComponentID = 2;
            //header.DestinationComponentID = 3;
            //header.SourceEntity = "CreditProfile";
            //customerInformation.Header = header;

            //Customer customer = new Customer();
            //customer.CustomerId = "60127439-4988-e411-80c6-0050587f2er3";
            //customer.CustomerNumber = "197";
            //customer.ParentCompanyId = "";

            //CreditProfile creditProfile = new CreditProfile();
            //creditProfile.CreditProfileId = "1470ec7a-1889-e511-80f9-0050569f7fa9";
            //creditProfile.CreditType = "";
            //creditProfile.PaymentTerms = "Pre Payment";
            //creditProfile.CorporateCreditType = "";
            //creditProfile.CreditHoldFlag = false;
            //creditProfile.CreditReviewRequiredFlag = true;
            //creditProfile.TerritoryExceptionFlag = false;
            //creditProfile.CustomerClass = "";
            //creditProfile.TaxCertStatus = "";
            //creditProfile.InheritCreditLimitfromParent = false;
            //customer.CreditProfile = creditProfile;

            //Status status = new Status();
            //status.StatusCode = "1";
            //status.Description = "Active";
            //status.StatusReason = "Active";
            //customer.Status = status;

            //AuditInformation auditInformation = new AuditInformation();
            //auditInformation.CreatedBy = "CHEMPOINT\\SQLAdminUser";
            //auditInformation.CreatedDate = "12/20/2014 1:08:08 PM";
            //auditInformation.ModifiedBy = "CHEMPOINT\\spalani";
            //auditInformation.ModifiedDate = "12/20/2014 1:08:08 PM";
            //customer.AuditInformation = auditInformation;

            //customer.CustomerStatus = "Active";
            //customer.Name = "Mechanical Rubber Products Co";
            //customer.Currency = CurrencyCode.USD;

            //Region region = new Region();
            //region.RegionID = "95928359-9164-e311-9b95-005056b00006";
            //region.Description = "North America";
            //customer.Region = region;

            //AddressInformation addressInformation = new Chempoint.BusinessObjects.AddressInformation();
            //addressInformation.AddressLine1 = "77 Forester Ave";
            //addressInformation.AddressLine2 = "";
            //addressInformation.City = "Warwick";
            //State state = new State();
            //state.StateID = "NY";
            //addressInformation.State = state;
            //addressInformation.ZipCode = "10990-1107";
            //Country country = new Country();
            //country.CountryID = "US";
            //addressInformation.Country = country;
            //customer.Address = addressInformation;

            //TelePhonesType telePhonesType = new TelePhonesType();
            //telePhonesType.MainPhone = "(221) 321-3213";
            //telePhonesType.MainPhoneExtension = "";
            //telePhonesType.MobilePhone = "";
            //telePhonesType.AlternatePhone = "";
            //telePhonesType.FaxNumber = "8459860399";
            //customer.PhoneNumber = telePhonesType;

            //Contact contact = new Contact();
            //contact.ContactId = "8bd4342a-7188-e411-80c6-0050569f2e8c";
            //contact.FirstName = "Nicole";
            //contact.LastName = "Cosamono";
            //contact.Salutation = "";
            //contact.Gender = "";
            //contact.PhoneNumbers = telePhonesType;
            //contact.Email = "raghunath.anipeddi@chempoint.com";
            //Status custStatus = new Status();
            //custStatus.StatusCode = "0";
            //custStatus.Description = "Active";
            //custStatus.StatusReason = "Active";
            //contact.Status = custStatus;
            //customer.PrimaryContact = contact;

            //customerInformation.CustomerAccount = customer;

            //AccountRequest accountRequest = new AccountRequest();
            //accountRequest.XrmCustomerInformation = customerInformation;

            //using (HttpClient client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri("http://localhost:63460/");
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    var response = client.PostAsJsonAsync("api/Customer/SaveAccountInGP/", accountRequest); // we need to refer the web.api service url here.
            //    if (response.Result.IsSuccessStatusCode)
            //    {
            //        AccountResponse accountResponse = response.Result.Content.ReadAsAsync<AccountResponse>().Result;
            //        if (accountResponse.Status == Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success)
            //        {
            //            isResult = true;
            //        }
            //        else
            //        {
            //            isResult = false;
            //        }
            //    }
            //    else
            //    {
            //        isResult = false;
            //    }
            //    if (!isResult)
            //    {

            //    }

            //    #endregion

            bool isResult = false;
            QuoteInformation quoteInformation = new QuoteInformation();

            quoteInformation.QuoteId = "aa401111-90fc-e511-8105-0050569f7fa9";
            quoteInformation.QuoteNumber = "3091748";
            quoteInformation.QuoteName = "2057755 DOWTHERM Q - 199.58 kg Drum";
            quoteInformation.RevisionNumber = "2";
            quoteInformation.Currency = CurrencyCode.USD;

            Customer customer = new Customer();
            customer.CustomerId = "b2aed50b-5188-e411-80c6-0050569f2e8c";
            customer.CustomerNumber = "782992";
            customer.ParentCompanyId = "";

            CreditProfile creditProfile = new CreditProfile();
            creditProfile.CreditProfileId = "1470ec7a-1889-e511-80f9-0050569f7fa9";
            creditProfile.CreditType = "";
            creditProfile.PaymentTerms = "Net 30";

            Amount amount = new Amount();
            amount.Amount1 = 90000;
            amount.CurrencyCode = CurrencyCode.USD;
            creditProfile.CreditLimit = amount;

            creditProfile.CorporateCreditType = "";
            creditProfile.CreditHoldFlag = false;
            creditProfile.CreditReviewRequiredFlag = true;
            creditProfile.TerritoryExceptionFlag = false;
            creditProfile.CustomerClass = "";
            creditProfile.TaxCertStatus = "";
            creditProfile.InheritCreditLimitfromParent = false;

            CreditProfileTaxDetail creditProfileTaxDetail = new Chempoint.BusinessObjects.CreditProfileTaxDetail();
            creditProfileTaxDetail.CustomerDeclaration = "Exempt";
            TaxInformation[] taxInformationArray = new TaxInformation[1];
            TaxInformation taxInformation = new TaxInformation();
            taxInformation.TaxId = "6791cca2-9703-e611-80cc-0050569f6243";
            State taxState = new State();
            taxState.StateID = "CA";
            taxInformation.State = taxState;
            Country taxCountry = new Country();
            taxCountry.CountryID = "US";
            taxInformation.Country = taxCountry;
            taxInformation.VATNumber = "DE127470856";
            taxInformation.IsValidtated = true;
            Status taxStatus = new Status();
            taxStatus.StatusCode = "0";
            taxStatus.Description = "Active";
            taxStatus.StatusReason = "Active";
            taxInformation.Status = taxStatus;
            taxInformationArray[0] = taxInformation;
            creditProfileTaxDetail.TaxInformations = taxInformationArray;
            creditProfile.TaxDetail = creditProfileTaxDetail;
            customer.CreditProfile = creditProfile;

            Status status = new Status();
            status.StatusCode = "1";
            status.Description = "Active";
            status.StatusReason = "Active";
            customer.Status = status;

            AuditInformation auditInformation = new AuditInformation();
            auditInformation.CreatedBy = "CHEMPOINT\\SQLAdminUser";
            auditInformation.CreatedDate = "12/20/2014 1:08:08 PM";
            auditInformation.ModifiedBy = "CHEMPOINT\\spalani";
            auditInformation.ModifiedDate = "12/20/2014 1:08:08 PM";
            customer.AuditInformation = auditInformation;

            customer.CustomerStatus = "Active";
            customer.Name = "Mechanical Rubber Products Co";
            customer.Currency = CurrencyCode.USD;

            Region region = new Region();
            region.RegionID = "95928359-9164-e311-9b95-005056b00006";
            region.Description = "North America";
            customer.Region = region;

            AddressInformation addressInformation = new Chempoint.BusinessObjects.AddressInformation();
            addressInformation.AddressLine1 = "77 Forester Ave";
            addressInformation.AddressLine2 = "";
            addressInformation.City = "Warwick";
            State state = new State();
            state.StateID = "NY";
            addressInformation.State = state;
            addressInformation.ZipCode = "10990-1107";
            Country country = new Country();
            country.CountryID = "US";
            addressInformation.Country = country;
            customer.Address = addressInformation;

            TelePhonesType telePhonesType = new TelePhonesType();
            telePhonesType.MainPhone = "(221) 321-3213";
            telePhonesType.MainPhoneExtension = "";
            telePhonesType.MobilePhone = "";
            telePhonesType.AlternatePhone = "";
            telePhonesType.FaxNumber = "8459860399";
            customer.PhoneNumber = telePhonesType;

            Contact contact = new Contact();
            contact.ContactId = "8bd4342a-7188-e411-80c6-0050569f2e8c";
            contact.FirstName = "Nicole";
            contact.LastName = "Cosamono";
            contact.Salutation = "";
            contact.Gender = "";
            contact.PhoneNumbers = telePhonesType;
            contact.Email = "raghunath.anipeddi@chempoint.com";
            Status custStatus = new Status();
            custStatus.StatusCode = "0";
            custStatus.Description = "Active";
            custStatus.StatusReason = "Active";
            contact.Status = custStatus;
            customer.PrimaryContact = contact;

            quoteInformation.MainAccount = customer;
            quoteInformation.BillingAccount = customer;
            quoteInformation.ShippingAccount = customer;
            quoteInformation.FinalDestinationAccount = customer;

            LineItemType lineItemType = new LineItemType();
            SKUType sKUType = new SKUType();
            sKUType.SkuNumber = "1124589";
            sKUType.Region = "North America";
            lineItemType.Sku = sKUType;
            quoteInformation.LineItem = lineItemType;

            Status quotestatus = new Status();
            quotestatus.StatusCode = "1";
            quotestatus.Description = "Active";
            quotestatus.StatusReason = "In Progress";
            quoteInformation.Status = quotestatus;

            AuditInformation quoteAuditInformation = new AuditInformation();
            quoteAuditInformation.CreatedBy = "CHEMPOINT\\SQLAdminUser";
            quoteAuditInformation.CreatedDate = "12/20/2014 1:08:08 PM";
            quoteAuditInformation.ModifiedBy = "CHEMPOINT\\spalani";
            quoteAuditInformation.ModifiedDate = "12/20/2014 1:08:08 PM";
            quoteInformation.AuditInformation = quoteAuditInformation;

            FreightInformation freightInformation = new FreightInformation ();
            freightInformation.FreightPerUnit = 0.00M;
            freightInformation.IsFIP =false;
            quoteInformation.ShippingInformation = freightInformation;

            Warehouse warehouse = new Warehouse();
            warehouse.WarehouseNumber = "698122";
            quoteInformation.ShipFromAccount = warehouse;

            AccountRequest accountRequest = new AccountRequest();
            accountRequest.XrmQuoteInformation = quoteInformation;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:63460/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("api/Customer/SaveQuoteInGP/", accountRequest); // we need to refer the web.api service url here.
                if (response.Result.IsSuccessStatusCode)
                {
                    AccountResponse accountResponse = response.Result.Content.ReadAsAsync<AccountResponse>().Result;
                    if (accountResponse.Status == Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success)
                    {
                        isResult = true;
                    }
                    else
                    {
                        isResult = false;
                    }
                }
                else
                {
                    isResult = false;
                }
                if (!isResult)
                {


                }

            }
        }
    }
}
