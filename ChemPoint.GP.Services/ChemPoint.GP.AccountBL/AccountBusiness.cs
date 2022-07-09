using Chempoint.GP.Infrastructure.Email;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Model.Interactions.Account;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Account;
using ChemPoint.GP.DataContracts.Account;
using Microsoft.Dynamics.GP.eConnect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Chempoint.GP.Infrastructure.Utils;

namespace ChemPoint.GP.AccountBL
{
    public class AccountBusiness : IAccountBI
    {
        /// <summary>
        /// To save the customer details from XRM to GP
        /// </summary>
        /// <param name="aRequest">Request object</param>
        /// <returns></returns>
        public AccountResponse SaveCustomerDetails(AccountRequest aRequest)
        {
            AccountResponse aResponse = null;
            StringBuilder logMessage = null;

            bool isSuccessfullyPushed = false;
            bool isCustomerExists = false;
            string errorMessage = string.Empty;
            eConnectMethods eConObj = null;
            string source = string.Empty;
            DataSet addressDs = null;
            string eConConnectionString = string.Empty;
            string connectionString = string.Empty;
            
            try
            {
                logMessage = new StringBuilder();
                aResponse = new AccountResponse();
                eConObj = new eConnectMethods();
                Chempoint.BusinessObjects.CustomerInformation accountDetails = aRequest.XrmCustomerInformation;


                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Customer Account Integration Started");

                if (accountDetails != null)
                {
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CustomerId in CRM : " + accountDetails.CustomerAccount.CustomerNumber);

                    source = accountDetails.Header.SourceEntity;
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Source : " + source);
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Currency Id : " + accountDetails.CustomerAccount.Currency);
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Customer Status : " + accountDetails.CustomerAccount.Status.Description);

                    if (accountDetails.CustomerAccount.Status.Description.ToLower() == "SunSetting")
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Since the customer status is sunsetting there is no update in GP");
                        isSuccessfullyPushed = true;
                    }
                    else
                    {
                        if (accountDetails.CustomerAccount.Currency.ToString() == "ZUS")
                        {
                            
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Currency Id : " + accountDetails.CustomerAccount.Currency);
                        }

                        if (String.IsNullOrEmpty(accountDetails.CustomerAccount.Region.Description) || accountDetails.CustomerAccount.Region.Description.ToLower() == "north america")
                        {
                            eConConnectionString = aRequest.NAEconnectConnectionString;
                            connectionString = aRequest.NAConnectionString;
                            aRequest.CompanyID = 1;
                            aRequest.CompanyName = "Chmpt";
                        }
                        else
                        {
                            eConConnectionString = aRequest.EUEconnectConnectionString;
                            connectionString = aRequest.EUConnectionString;
                            aRequest.CompanyID = 2;
                            aRequest.CompanyName = "Cpeur";
                        }

                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "EconConnectionString :" + eConConnectionString);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "ConnectionString :" + connectionString);

                        IAccountRepository accountDataAccess = new ChemPoint.GP.Account.AccountDL.AccountDL(connectionString);
                        if (source != "CreditProfile")
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling the SP to get the addressId's");
                            addressDs = (DataSet)accountDataAccess.FetchAccountDetails(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Result of the SP to get the addressId's" + addressDs.ToString());
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-Customer Status- " + accountDetails.CustomerAccount.Status.Description.ToLower());
                        }
                        if (accountDetails.CustomerAccount.Status.Description.ToLower() == "inactive")
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling SP to check whether the customer is having open transaction or not");
                            string message = accountDataAccess.GetCustomerOpenTransactionStatus(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Message: " + message);

                            if (string.IsNullOrEmpty(message))
                            {
                                if (source != "CreditProfile")
                                {
                                    if (addressDs.Tables[0] != null && addressDs.Tables[0].Rows.Count > 0)
                                    {
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- Account Reference Count " + addressDs.Tables[0].Rows.Count.ToString());
                                        StringBuilder accountIds = new StringBuilder();
                                        foreach (DataRow item in addressDs.Tables[0].Rows)
                                        {
                                            accountIds.Append(item[1] + "(" + (item[3].ToString() == "1" ? "Billing" : "Shipping") + ") ,");//CustomerId is available in the index 1 and addresstype is available in the index 3
                                        }
                                        isSuccessfullyPushed = false;
                                        errorMessage = "The customer " + accountDetails.CustomerAccount.CustomerNumber + " is been referenced in " + accountIds + ".";
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + errorMessage);
                                    }
                                    else
                                    {
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Open Transaction does not exists for the customer " + accountDetails.CustomerAccount.CustomerNumber);
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling SP to deactivate the customer");
                                        accountDataAccess.DeactivateCustomerinGP(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Customer is deactivated successfully");
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + " Try to deactivate Credit Profile from XRM");
                                }
                            }
                            else
                            {
                                isSuccessfullyPushed = false;
                                errorMessage = "Open Transaction exists for the customer " + accountDetails.CustomerAccount.CustomerNumber;
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + errorMessage);
                            }
                        }
                        else if (accountDetails.CustomerAccount.Status.Description.ToLower() == "active")
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting object into XML");
                            string inputXml = SerializeToString(accountDetails);
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Input xml : ");
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + inputXml);

                            if (inputXml != string.Empty)
                            {
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Check whether the customer exists in GP or not");
                                isCustomerExists = accountDataAccess.IsCustomerExistsInGP(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Is Customer ID " + accountDetails.CustomerAccount.CustomerNumber + " Exists in GP or not : " + isCustomerExists.ToString());

                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting inputxml into econnect xml");
                                string econnectXml = Transform(inputXml, source, isCustomerExists, ref logMessage, aRequest.QuoteStyleSheetPath,
                                    aRequest.AccountStyleSheetPath, aRequest.RemoveNamespaceStyleSheetPath);
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect Xml: " + econnectXml);

                                if (!string.IsNullOrEmpty(econnectXml))
                                {
                                    if (isCustomerExists)
                                    {
                                        #region Juri. States and Tax Declaration Status - Original
                                        
                                        DataSet originalRequestDetails = new DataSet { Locale = CultureInfo.InvariantCulture };
                                        if (accountDetails.CustomerAccount.Region.Description.ToLower() == "north america")
                                        {
                                            originalRequestDetails = (DataSet)accountDataAccess.FetchAvalaraRequestDetails(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                                            if (originalRequestDetails == null || originalRequestDetails.Tables.Count == 0)
                                            {
                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Jurisdiction details does not exists before updation");
                                            }
                                        }

                                        #endregion Juri. States and Tax Declaration Status - Original
                                        
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect object created successfully");
                                        isSuccessfullyPushed = eConObj.CreateEntity(eConConnectionString, econnectXml);
                                        
                                        if (isSuccessfullyPushed == true)
                                        {
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Customer pushed successfully in to GP.");
                                            
                                            #region Avalara Region
                                                
                                            if (accountDetails.CustomerAccount.Region.Description.ToLower() == "north america")
                                            {
                                                string customerlogMessage = "";
                                                string emailAddress = "";
                                                try
                                                {
                                                    if (accountDetails.CustomerAccount.PrimaryContact != null)
                                                    {
                                                        if (accountDetails.CustomerAccount.PrimaryContact.Email != null)
                                                        {
                                                            emailAddress = accountDetails.CustomerAccount.PrimaryContact.Email.ToString().Trim();
                                                            if (emailAddress == "")
                                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Receive blank email id for customer: " + accountDetails.CustomerAccount.CustomerNumber);
                                                        }
                                                    }
                                                }
                                                catch (Exception exc)
                                                {
                                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error in fetching email id " + exc.Message);
                                                }
                                                
                                                DataSet updatedRequestDetails = new DataSet { Locale = CultureInfo.InvariantCulture };
                                                updatedRequestDetails = (DataSet)accountDataAccess.FetchAvalaraRequestDetails(accountDetails.CustomerAccount.CustomerNumber, aRequest.CompanyID);
                                                if (updatedRequestDetails == null || updatedRequestDetails.Tables.Count == 0)
                                                {
                                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Jurisdiction details does not exists after updation");
                                                }
                                                    
                                                if (originalRequestDetails.Tables[0] != null && updatedRequestDetails.Tables[0] != null)
                                                {
                                                    if (originalRequestDetails.Tables[1] != null && updatedRequestDetails.Tables[1] != null)
                                                    {
                                                        var queryUpdatedStates = updatedRequestDetails.Tables[1].AsEnumerable().Select(a => new { exposureZoneName = a["Jurisdiction"].ToString() });
                                                        var queryOriginalStates = originalRequestDetails.Tables[1].AsEnumerable().Select(a => new { exposureZoneName = a["Jurisdiction"].ToString() });
                                                            
                                                        if (updatedRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt"))
                                                        {
                                                            var resultStates = queryUpdatedStates.Except(queryOriginalStates);
                                                            if (resultStates.Count() > 0)
                                                            {
                                                                string exposureZoneName = "";
                                                                foreach (var states in resultStates)
                                                                {
                                                                    exposureZoneName = states.exposureZoneName + "," + exposureZoneName;
                                                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Avalara: Sending Cert Request for exposure zone: " + states.exposureZoneName + " for email id: " + emailAddress);
                                                                }
                                                                SendCertRequestToAvalara(accountDetails.CustomerAccount.CustomerNumber, exposureZoneName.TrimEnd(','), emailAddress, aRequest.AvalaraWebServiceUrl, connectionString, out customerlogMessage);
                                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + customerlogMessage);
                                                            }
                                                        }
                                                        
                                                        if (!updatedRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt") &&
                                                            originalRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt"))
                                                        {
                                                            var resultStates = queryOriginalStates.Except(queryUpdatedStates);
                                                            if (resultStates.Count() > 0)
                                                            {
                                                                StringBuilder avaMailContent = new StringBuilder();
                                                                avaMailContent.AppendLine(aRequest.AvalaraEmail.Body);
                                                                avaMailContent.Replace("CustomerNumber", accountDetails.CustomerAccount.CustomerNumber);
                                                                
                                                                avaMailContent.Replace("</table>", "").Replace("</html>", "");
                                                                
                                                                int exposureZoneID = 1;
                                                                foreach (var states in resultStates)
                                                                {
                                                                    avaMailContent.AppendLine("<tr><td>" + exposureZoneID +
                                                                                              "</td><td>" + accountDetails.CustomerAccount.CustomerNumber +
                                                                                              "</td><td>" + states.exposureZoneName +
                                                                                              "</td><td>" + "In Active" +
                                                                                              "</td></tr>");

                                                                    exposureZoneID = exposureZoneID + 1;
                                                                }
                                                                
                                                                // Sends email if status changes from Exempt to Anything
                                                                if (avaMailContent.ToString().Contains("<tr><td>"))
                                                                {
                                                                    avaMailContent.Append("</table></html>");
                                                                    try
                                                                    {
                                                                        aRequest.AvalaraEmail.Body = avaMailContent.ToString();
                                                                        new EmailHelper().SendMail(aRequest.AvalaraEmail);
                                                                    
                                                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Mail Sent for Tax Exempt Status Changed");
                                                                    }
                                                                    catch (Exception exc)
                                                                    {
                                                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while sending email for tax exempt status changes: " + exc.Message);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                    
                                            #endregion Avalara Region
                                        }
                                    }
                                    else
                                        isSuccessfullyPushed = true;
                                        
                                    if (isSuccessfullyPushed && source != "CreditProfile" && addressDs.Tables[0] != null && addressDs.Tables[0].Rows.Count > 0)
                                    {
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "starting pushing the customer adderss details to the references.");
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting object into XML");
                                        string addressXml = addressDs.GetXml();
                                        if (!string.IsNullOrEmpty(addressXml))
                                        {
                                            addressXml = addressXml.Replace("<NewDataSet>", "<CustomerInformation><RefAddress>")
                                                                   .Replace("</NewDataSet>", "</RefAddress></CustomerInformation>")
                                                                   .Replace("<Table>", "<AddressType>")
                                                                   .Replace("</Table>", "</AddressType>");
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "address Input xml : ");
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + addressXml);
                                                
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting address xml into econnect xml");
                                            econnectXml = Transform(addressXml, "AddressRef", accountDetails.CustomerAccount.Address.AddressLine1,
                                                accountDetails.CustomerAccount.Address.AddressLine2, accountDetails.CustomerAccount.Address.Country.CountryID, accountDetails.CustomerAccount.Address.City,
                                                accountDetails.CustomerAccount.Address.State.StateID,
                                                accountDetails.CustomerAccount.Address.ZipCode, accountDetails.CustomerAccount.PhoneNumber.MainPhone, aRequest.AccountStyleSheetPath, ref logMessage);
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect Xml: " + econnectXml);
                                            if (!string.IsNullOrEmpty(econnectXml))
                                            {
                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling econnect object");
                                                isSuccessfullyPushed = eConObj.CreateEntity(eConConnectionString, econnectXml);
                                                
                                                if (isSuccessfullyPushed == true)
                                                {
                                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Reference Address Details are pushed successfully in to GP.");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnectxml is not generated");
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while converting object into XML");
                            }
                        }
                    }
                }
            }
            catch (eConnectException econEx)
            {
                isSuccessfullyPushed = false;
                if (econEx.Message.Contains("Error Number = "))
                {
                    try
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Status : Error while pushing customer into GP");
                    
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error Code: " + Convert.ToInt32(econEx.Message.Substring(econEx.Message.IndexOf("Error Number = ") + 15,
                            (econEx.Message.IndexOf(" ", econEx.Message.IndexOf("Error Number = ") + 15) - (econEx.Message.IndexOf("Error Number = ") + 15)))));
                    }
                    catch (Exception ex)
                    {
                        errorMessage = econEx.Message.Trim();
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error: " + ex.Message);
                    }
                }
                else
                {
                    errorMessage = econEx.Message.Trim();
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error: " + econEx.Message);
                }
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect Error: " + econEx.Message);
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + econEx.StackTrace);
                //throw;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + ex.Message);
                errorMessage = ex.Message;
                isSuccessfullyPushed = false;
            }
            finally
            {
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Customer Account Integration ended.");
                
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), aRequest.LoggingPath, aRequest.LoggingFileName);
            
                aResponse.Status = (isSuccessfullyPushed ? Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success : Chempoint.GP.Model.Interactions.Account.ResponseStatus.Error);
                aResponse.ErrorMessage = errorMessage;
            }
            return aResponse;
        }
        
        /// <summary>
        /// Create customer while activating the quote in CRM
        /// </summary>
        /// <param name="accountDetails"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public AccountResponse SaveQuoteDetailsIntoGP(AccountRequest aRequest)
        {
            AccountResponse aResponse = null;
            StringBuilder logMessage = new StringBuilder();
            bool isSuccessfullyPushed = false;
            bool isCustomerExists = false;
            string errorMessage = string.Empty;
            int errorCode = 0;
            
            eConnectMethods eConObj = null;
            string source = string.Empty;
            
            string eConConnectionString = string.Empty;
            string connectionString = string.Empty;
                
            try
            {
                aResponse = new AccountResponse();
                Chempoint.BusinessObjects.QuoteInformation quoteDetails = aRequest.XrmQuoteInformation;
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Quote Integration Started");
                source = "Quote";
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Source : " + source);
                
                 
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "QuoteStatus : " + quoteDetails.MainAccount.Status.StatusReason);
                if (quoteDetails != null)
                {
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Currency Id : " + quoteDetails.Currency.ToString());
                    if (quoteDetails.Currency.ToString() == "ZUS")
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Currency Id : " + quoteDetails.Currency);
                    }
                        
                    if (quoteDetails.LineItem != null && String.IsNullOrEmpty(quoteDetails.LineItem.Sku.Region) || quoteDetails.LineItem.Sku.Region.ToLower() == "north america")
                    {
                        eConConnectionString = aRequest.NAEconnectConnectionString;
                        connectionString = aRequest.NAConnectionString;
                        aRequest.CompanyID = 1;
                    }
                    else
                    {
                        eConConnectionString = aRequest.EUEconnectConnectionString;
                        connectionString = aRequest.EUConnectionString;
                        aRequest.CompanyID = 2;
                    }
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "EconConnectionString :" + eConConnectionString);
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "ConnectionString :" + connectionString);
                        
                    IAccountRepository accountDataAccess = new ChemPoint.GP.Account.AccountDL.AccountDL(connectionString);
                    if (quoteDetails.Status.Description.ToLower() == "active")
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "MainCustomerId in GP : " + quoteDetails.MainAccount.CustomerNumber);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "BillingCustomerId in GP : " + quoteDetails.BillingAccount.CustomerNumber);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "ShippingCustomerId in GP : " + quoteDetails.ShippingAccount.CustomerNumber);
                        
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting object into XML");
                        string inputXml = SerializeToString(quoteDetails);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Quote Input xml : ");
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + inputXml);
                        
                        isCustomerExists = accountDataAccess.IsCustomerExistsInGP(quoteDetails.MainAccount.CustomerNumber, aRequest.CompanyID);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "MainAccountCustomer ID " + quoteDetails.MainAccount.CustomerNumber + " Exists in GP or not : " + isCustomerExists.ToString());
                            
                        if (inputXml != string.Empty)
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting inputxml into econnect xml");
                            string econnectXml = Transform(inputXml, source, isCustomerExists, ref logMessage, aRequest.QuoteStyleSheetPath,
                                aRequest.AccountStyleSheetPath, aRequest.RemoveNamespaceStyleSheetPath);
                                
                            if (!string.IsNullOrEmpty(econnectXml))
                            {
                                #region Juri. States and Tax Declaration Status - Original
                                    
                                DataSet originalRequestDetails = new DataSet { Locale = CultureInfo.InvariantCulture };
                                if (quoteDetails.LineItem.Sku.Region.ToLower() == "north america")
                                {
                                    originalRequestDetails = (DataSet)accountDataAccess.FetchAvalaraRequestDetails(quoteDetails.MainAccount.CustomerNumber, aRequest.CompanyID); 
                                    if (originalRequestDetails == null || originalRequestDetails.Tables.Count == 0)
                                    {
                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Jurisdiction details does not exists before updation");
                                    }
                                }
                                
                                #endregion Juri. States and Tax Declaration Status - Original
                                
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect Xml: " + econnectXml);
                                eConObj = new eConnectMethods();
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect object created successfully");
                                isSuccessfullyPushed = eConObj.CreateEntity(eConConnectionString, econnectXml);
                                if (isSuccessfullyPushed == true)
                                {
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Customer pushed successfully in to GP.");
                                        
                                    #region Avalara Region
                                            
                                    if (quoteDetails.LineItem.Sku.Region.ToLower() == "north america")
                                    {
                                        string customerlogMessage = "";
                                        string emailAddress = "";
                                        try
                                        {
                                            if (quoteDetails.MainAccount.PrimaryContact != null)
                                            {
                                                if (quoteDetails.MainAccount.PrimaryContact.Email != null)
                                                {
                                                    emailAddress = quoteDetails.MainAccount.PrimaryContact.Email.ToString().Trim();
                                                    if (emailAddress == "")
                                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Receive blank email id for customer: " + quoteDetails.MainAccount.CustomerNumber);
                                                }
                                            }
                                        }
                                        catch (Exception exc)
                                        {
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error in fetching email id " + exc.Message);
                                        }
                                        
                                        if (!isCustomerExists)
                                        {
                                            PushCustomerIntoAvalara(quoteDetails.MainAccount.CustomerNumber, emailAddress, aRequest.AvalaraWebServiceUrl, aRequest.CompanyID, ref logMessage, ref accountDataAccess);
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + customerlogMessage);
                                        }

                                        DataSet updatedRequestDetails = new DataSet { Locale = CultureInfo.InvariantCulture };
                                        updatedRequestDetails = (DataSet)accountDataAccess.FetchAvalaraRequestDetails(quoteDetails.MainAccount.CustomerNumber, aRequest.CompanyID); 
                                        if (updatedRequestDetails == null || updatedRequestDetails.Tables.Count == 0)
                                        {
                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Jurisdiction details does not exists after updation");
                                        }

                                        if (originalRequestDetails.Tables[0] != null && updatedRequestDetails.Tables[0] != null)
                                        {
                                            if (originalRequestDetails.Tables[1] != null && updatedRequestDetails.Tables[1] != null)
                                            {
                                                var queryUpdatedStates = updatedRequestDetails.Tables[1].AsEnumerable().Select(a => new { exposureZoneName = a["Jurisdiction"].ToString() });
                                                var queryOriginalStates = originalRequestDetails.Tables[1].AsEnumerable().Select(a => new { exposureZoneName = a["Jurisdiction"].ToString() });
                                                        
                                                if (updatedRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt"))
                                                {
                                                    var resultStates = queryUpdatedStates.Except(queryOriginalStates);
                                                            
                                                    if (resultStates.Count() > 0)
                                                    {
                                                        string exposureZoneName = "";
                                                        foreach (var states in resultStates)
                                                        {
                                                            exposureZoneName = states.exposureZoneName + "," + exposureZoneName;
                                                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Avalara: Sending Cert Request for exposure zone: " + states.exposureZoneName + " for email id: " + emailAddress);
                                                        }
                                                        SendCertRequestToAvalara(quoteDetails.MainAccount.CustomerNumber, exposureZoneName.TrimEnd(','), emailAddress, aRequest.AvalaraWebServiceUrl, connectionString, out customerlogMessage);
                                                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + customerlogMessage);
                                                    }
                                                }
                                                if (!updatedRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt") &&
                                                    originalRequestDetails.Tables[0].Rows[0]["TaxDeclarationStatus"].Equals("Exempt"))
                                                {
                                                    var resultStates = queryOriginalStates.Except(queryUpdatedStates);
                                                    if (resultStates.Count() > 0)
                                                    {
                                                        StringBuilder avaMailContent = new StringBuilder();
                                                        avaMailContent.AppendLine(aRequest.AvalaraEmail.Body);
                                                        avaMailContent.Replace("CustomerNumber", quoteDetails.MainAccount.CustomerNumber);
                                                                                      
                                                        avaMailContent.Replace("</table>", "").Replace("</html>", "");
                                                                                      
                                                        int exposureZoneID = 1;
                                                        foreach (var states in resultStates)
                                                        {
                                                            avaMailContent.AppendLine("<tr><td>" + exposureZoneID +
                                                                                      "</td><td>" + quoteDetails.MainAccount.CustomerNumber +
                                                                                      "</td><td>" + states.exposureZoneName +
                                                                                      "</td><td>" + "In Active" +
                                                                                      "</td></tr>");
                                                            
                                                            exposureZoneID = exposureZoneID + 1;
                                                        }
                                                                
                                                        // Sends email if status changes from Exempt to Anything
                                                        if (avaMailContent.ToString().Contains("<tr><td>"))
                                                        {
                                                            avaMailContent.Append("</table></html>");
                                                            try
                                                            {
                                                                aRequest.AvalaraEmail.Body = avaMailContent.ToString();
                                                                new EmailHelper().SendMail(aRequest.AvalaraEmail);
                                                        
                                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Mail Sent for Tax Exempt Status Changed");
                                                            }
                                                            catch (Exception exc)
                                                            {
                                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while sending email for tax exempt status changes: " + exc.Message);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                        
                                    #endregion Avalara Region
                                }
                            }
                            else
                            {
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnectxml is not generated");
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while converting object into XML");
                        }
                    }
                    else if (quoteDetails.Status.Description.ToLower() == "closed")
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Converting object into XML");
                        string inputXml = SerializeToString(quoteDetails);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Quote Input xml : ");
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + inputXml);
                       
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "DeActivating the quote details");
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "StatusQuote:" + quoteDetails.Status.StatusCode);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "StatusReason:" + quoteDetails.Status.StatusReason);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "QuoteId:" + quoteDetails.QuoteId);
                        accountDataAccess.DeactivateQuoteinGP(quoteDetails.QuoteId, quoteDetails.Status.StatusCode, quoteDetails.Status.StatusReason, aRequest.CompanyID);
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Quote DeActivated Succesfully");
                    }
                }
                else
                {
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Input object is null");
                    errorMessage = "Input object is null";
                }
            }
            catch (eConnectException econEx)
            {
                isSuccessfullyPushed = false;
                if (econEx.Message.Contains("Error Number = "))
                {
                    try
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Status : Error while pushing customer into GP");
                        
                        errorCode = Convert.ToInt32(econEx.Message.Substring(econEx.Message.IndexOf("Error Number = ") + 15,
                            (econEx.Message.IndexOf(" ", econEx.Message.IndexOf("Error Number = ") + 15) - (econEx.Message.IndexOf("Error Number = ") + 15))));
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error Code: " + errorCode);
                    }
                    catch (Exception ex)
                    {
                        errorMessage = econEx.Message.Trim();
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error: " + ex.Message);
                    }
                }
                else
                {
                    errorMessage = econEx.Message.Trim();
                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error: " + econEx.Message);
                }
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Econnect Error: " + econEx.Message);
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + econEx.StackTrace);
                //throw;
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + ex.Message);
                errorMessage = ex.Message;
                isSuccessfullyPushed = false;
            }
            finally
            {
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "CRM Quote Integration ended.");
        
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), aRequest.LoggingPath, aRequest.LoggingFileName);
        
                aResponse.Status = (isSuccessfullyPushed ? Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success : Chempoint.GP.Model.Interactions.Account.ResponseStatus.Error);
                aResponse.ErrorMessage = errorMessage;
            }
            return aResponse;
        }
        
        /// <summary>
        /// Validate the customer is having any open transaction or not
        /// </summary>
        /// <param name="quoteDetails"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public AccountResponse IsOpenTransactionExistsForCustomer(AccountRequest aRequest)
        {
            StringBuilder logMessage = null;
            AccountResponse response = new AccountResponse();
            string message = string.Empty;
                
            bool ifExists = false;
            string connectionString = string.Empty;
            try
            {
                logMessage = new StringBuilder();
                logMessage.AppendLine(DateTime.Now.ToString() + " - CRM Customer Status Validation Started.");
                    
                if (!String.IsNullOrEmpty(aRequest.GPCustomerInformation.CustomerId))
                {
                    if (aRequest.CompanyID == 1)
                        connectionString = aRequest.NAConnectionString;
                    else
                        connectionString = aRequest.EUConnectionString;
                    
                    IAccountRepository accountDataAccess = new ChemPoint.GP.Account.AccountDL.AccountDL(connectionString);
                    
                    logMessage.AppendLine(DateTime.Now.ToString() + " - ConnectionString :" + connectionString);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Calling SP to validate the customer status");
                    
                    message = accountDataAccess.GetCustomerOpenTransactionStatus(aRequest.GPCustomerInformation.CustomerId, aRequest.CompanyID);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Message : " + message);
                    aRequest.AccountXml = message;
                    if (!string.IsNullOrEmpty(message) && message.Contains("does not exists in GP"))
                    {
                        ifExists = false;
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Status : " + ifExists);
                    }
                    else if (!string.IsNullOrEmpty(message))
                    {
                        ifExists = true;
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Status : " + ifExists);
                    }
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Customer Id is empty");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(ex.Message);
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - CRM Customer Status Validation ended.");
                
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), aRequest.LoggingPath, aRequest.LoggingFileName);
                if (ifExists)
                    response.Status = Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success;
                else
                    response.Status = Chempoint.GP.Model.Interactions.Account.ResponseStatus.Error;
        
                response.ErrorMessage = message;
            }
        
            return response;
        }
            
        /// <summary>
        /// Warehouse Deactivation status method...
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public AccountResponse GetWarehouseDeactivationStatus(AccountRequest aRequest)
        {
            StringBuilder logMessage = null;
            AccountResponse response = new AccountResponse();
            int message = -1;
            string connectionString = string.Empty;
            try
            {
                logMessage = new StringBuilder();
                logMessage.AppendLine(DateTime.Now.ToString() + " - CRM Warehouse Status Validation Started.");
                    
                if (!String.IsNullOrEmpty(aRequest.GPWarehouseInformation.WarehouseId) && !String.IsNullOrEmpty(aRequest.CurrencyId))
                {
                    if (aRequest.CompanyID == 1)
                        connectionString = aRequest.NAConnectionString;
                    else
                        connectionString = aRequest.EUConnectionString;
                        
                    IAccountRepository accountDataAccess = new ChemPoint.GP.Account.AccountDL.AccountDL(connectionString);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - ConnectionString :" + connectionString);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Calling SP to validate the Warehouse status");
                
                    message = accountDataAccess.GetWarehouseDeactivationStatus(aRequest);
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Message : " + message);
                    if (message == 0 || message == 4) //Changes made as directed by CRM 
                        response.Status = Chempoint.GP.Model.Interactions.Account.ResponseStatus.Error;
                    else
                        response.Status = Chempoint.GP.Model.Interactions.Account.ResponseStatus.Success;
                    logMessage.AppendLine("Message : Warehouse status is " + response.Status);
                }
                else
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Warehouse Id / Currency Id is empty");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(ex.Message);
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - CRM Customer Warehouse Validation ended.");
        
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), aRequest.LoggingPath, aRequest.LoggingFileName);
            }
            return response;
        }
                
        private static string SerializeToString(Object reference)
        {
            // creates the serializer object 
            XmlSerializer serializer = new XmlSerializer(reference.GetType());
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, reference);
                return writer.ToString();
            }
        }
        
        /// <summary>
        /// Convert an xml into econnect xml
        /// </summary>
        /// <param name="inputXml"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="docNumb"></param>
        /// <param name="bachNumb"></param>
        /// <returns></returns>
        private string Transform(string inputXml, string source, bool ifCustomerExists, ref StringBuilder logMessage,
            string quoteXsltPath, string customerXsltPath, string removeNameSpaceXsltPath)
        {
            XmlDocument xmlDoc = null;
            XslCompiledTransform xslTrans = null;
            XsltArgumentList xsltArgsPO = null;
                
            // Local variables.
            string transformedXml = string.Empty;
            StringWriter sWriter = null;
            string xslPath = string.Empty;
            try
            {
                //Create object for XmlDocument
                xmlDoc = new XmlDocument();
                
                //Create object for XslTransform
                xslTrans = new XslCompiledTransform();

                //Loading ChemMsgEcomOrder XML
                xmlDoc.LoadXml(inputXml);
                
                //Creating Argument List Object
                xsltArgsPO = new XsltArgumentList();
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT Params: Source : " + source);
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT Params: ifExists : " + ifCustomerExists);
                    
                //-----------------------------code to remove namespace from xml----------------------------
                xslPath = removeNameSpaceXsltPath;
                try
                {
                    xslTrans.Load(xslPath);
                    sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
                    xslTrans.Transform(xmlDoc, xsltArgsPO, sWriter);
                    transformedXml = sWriter.ToString().Trim();
                    sWriter.Dispose();
                }
                catch (Exception ex)
                {
                    transformedXml = "";
                    logMessage.AppendLine(ex.Message);
                }
                finally
                {
                    xmlDoc = null;
                    xslTrans = null;
                }
                //-----------------------------code to remove namespace from xml----------------------------
                if (source == "CustomerInformation" || source == "CreditProfile")
                {
                    xsltArgsPO.AddParam("Source", "", source.Trim());
                    xsltArgsPO.AddParam("Exists", "", (ifCustomerExists == true ? "Yes" : "No"));
                    xslPath = customerXsltPath;
                }
                else if (source == "Quote")
                {
                    xsltArgsPO.AddParam("IsCustomerExists", "", (ifCustomerExists == true ? "Yes" : "No"));
                    xslPath = quoteXsltPath;
                }
                xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(transformedXml);
                //Loading XSL
                xslTrans = new XslCompiledTransform();
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT Path: " + xslPath);
                xslTrans.Load(xslPath);
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT loaded successfully");
                
                //Creating StringWriter Object
                sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
                
                //Peroforming transformation
                xslTrans.Transform(xmlDoc, xsltArgsPO, sWriter);
            
                // Set the transformed xml.
                transformedXml = sWriter.ToString().Trim();
            
                // Dispose the objects.
                sWriter.Dispose();
            }
            catch (Exception ex)
            {
                transformedXml = "";
                logMessage.AppendLine(ex.Message);
            }
            finally
            {
                xmlDoc = null;
                xslTrans = null;
                xsltArgsPO = null;
            }
            // Return the transformed xml to the caller.
            return transformedXml;
        }
        
        /// <summary>
        /// Convert an xml into econnect xml
        /// </summary>
        /// <param name="inputXml"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="docNumb"></param>
        /// <param name="bachNumb"></param>
        /// <returns></returns>
        private string Transform(string inputXml, string source, string address1, string address2,
            string country, string city, string state, string postalCode, string phoneNumber, string customerStylesheetPath, ref StringBuilder logMessage)
        {
            XmlDocument xmlDoc = null;
            XslCompiledTransform xslTrans = null;
            XsltArgumentList xsltArgsPO = null;
                
            // Local variables.
            string transformedXml = string.Empty;
            StringWriter sWriter = null;
            string xslPath = string.Empty;
            try
            {
                //Create object for XmlDocument
                xmlDoc = new XmlDocument();
                
                //Create object for XslTransform
                xslTrans = new XslCompiledTransform();
                
                //Loading ChemMsgEcomOrder XML
                xmlDoc.LoadXml(inputXml);
                    
                //Creating Argument List Object
                xsltArgsPO = new XsltArgumentList();
                    
                if (source == "AddressRef")
                {
                    xsltArgsPO.AddParam("Source", "", source.Trim());
                    xsltArgsPO.AddParam("Exists", "", "");
                    xsltArgsPO.AddParam("Address1", "", address1.Trim());
                    xsltArgsPO.AddParam("Address2", "", address2.Trim());
                    xsltArgsPO.AddParam("Country", "", country.Trim());
                    xsltArgsPO.AddParam("City", "", city.Trim());
                    xsltArgsPO.AddParam("State", "", state.Trim());
                    xsltArgsPO.AddParam("PostalCode", "", postalCode.Trim());
                    xsltArgsPO.AddParam("PhoneNumber", "", phoneNumber.Trim());
                    xslPath = customerStylesheetPath;
                }
                
                //Loading XSL
                
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT Path: " + xslPath);
                xslTrans.Load(xslPath);
                logMessage.AppendLine(DateTime.Now.ToString() + " - XSLT loaded successfully");
                
                //Creating StringWriter Object
                sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
                
                //Peroforming transformation
                xslTrans.Transform(xmlDoc, xsltArgsPO, sWriter);
            
                // Set the transformed xml.
                transformedXml = sWriter.ToString().Trim();
            
                // Dispose the objects.
                sWriter.Dispose();
            }
            catch (Exception ex)
            {
                transformedXml = "";
                logMessage.AppendLine(ex.Message);
            }
            finally
            {
                xmlDoc = null;
                xslTrans = null;
                xsltArgsPO = null;
            }
            // Return the transformed xml to the caller.
            return transformedXml;
        }
                
        private bool SendCertRequestToAvalara(string custnmbr, string exposureZoneName, string emailAddress, string avaServiceUrl, string connectionString, out string logFileMessage)
        {
            logFileMessage = "";
            bool isSuccess = false;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                Dictionary<string, string> certificateDetails = new Dictionary<string, string>();
                certificateDetails.Add("customer", custnmbr); //
                certificateDetails.Add("state", exposureZoneName); //
                certificateDetails.Add("email_address", emailAddress); //
                
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = Int32.MaxValue;
                binding.MaxBufferSize = Int32.MaxValue;
                binding.MaxBufferPoolSize = Int32.MaxValue;
                binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
                
                //Calling Ava Alert Service
                AvaService.AvaServiceClient avaClient = new AvaService.AvaServiceClient(binding, new EndpointAddress(avaServiceUrl));
                isSuccess = avaClient.SendCertRequestToAvalara(certificateDetails, out logFileMessage);
                logMessage.AppendLine(logFileMessage);
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error In SendCertRequestToAvalara Method: " + ex.Message.ToString());
            }
            finally
            {
                logFileMessage = logMessage.ToString();
            }
            return isSuccess;
        }
                
        private bool PushCustomerIntoAvalara(string custnmbr, string emailAddress, string avaServiceUrl, int companyID, ref StringBuilder logMessage, ref IAccountRepository accountDataAccess)
        {
            bool isSuccess = false;
            try
            {
                DataTable avalaraDetails = new DataTable { Locale = CultureInfo.InvariantCulture };
                avalaraDetails = ((DataSet)accountDataAccess.FetchCustomerDetailsToPushToAvalara("Customers", custnmbr, companyID)).Tables[0];
                string logFileMessage;
                        
                if (avalaraDetails != null && avalaraDetails.Rows.Count > 0)
                {
                    if (Convert.ToInt16(avalaraDetails.Rows[0]["IsExistInAvalara"]) == 0)
                    {
                        Dictionary<string, string> customerDetails = new Dictionary<string, string>();
                        customerDetails.Add("customer_number", avalaraDetails.Rows[0]["CustomerNumber"].ToString().Trim()); // Customer Number
                        customerDetails.Add("alternate_id", avalaraDetails.Rows[0]["AlternateNumber"].ToString().Trim()); //  set as blank
                        customerDetails.Add("name", avalaraDetails.Rows[0]["CustomerName"].ToString().Trim()); //  Customer Name
                        customerDetails.Add("attn_name", avalaraDetails.Rows[0]["AttentionName"].ToString().Trim()); // Statement Name
                        customerDetails.Add("address_line1", avalaraDetails.Rows[0]["Address1"].ToString().Trim()); // Address1
                        customerDetails.Add("address_line2", avalaraDetails.Rows[0]["Address2"].ToString().Trim()); // Address2
                        customerDetails.Add("city", avalaraDetails.Rows[0]["City"].ToString().Trim()); // City
                        customerDetails.Add("zip", avalaraDetails.Rows[0]["Zip"].ToString().Trim()); // Zip
                        customerDetails.Add("phone_number", avalaraDetails.Rows[0]["Phone1"].ToString().Trim()); // Phone
                        customerDetails.Add("fax_number", avalaraDetails.Rows[0]["Fax"].ToString().Trim()); // Fax
                        customerDetails.Add("email_address", emailAddress); // Email
                        customerDetails.Add("contact_name", avalaraDetails.Rows[0]["ContactPerson"].ToString().Trim()); // Contact Person Name
                        customerDetails.Add("country", avalaraDetails.Rows[0]["Country"].ToString().Trim()); //   set as Country Code
                        customerDetails.Add("state", avalaraDetails.Rows[0]["State"].ToString().Trim()); //     set as State
                        
                        BasicHttpBinding binding = new BasicHttpBinding();
                        binding.MaxReceivedMessageSize = Int32.MaxValue;
                        binding.MaxBufferSize = Int32.MaxValue;
                        binding.MaxBufferPoolSize = Int32.MaxValue;
                        binding.ReaderQuotas.MaxArrayLength = Int32.MaxValue;
                        binding.ReaderQuotas.MaxBytesPerRead = Int32.MaxValue;
            
                        //Calling Ava Alert Service
                        AvaService.AvaServiceClient avaClient = new AvaService.AvaServiceClient(binding, new EndpointAddress(avaServiceUrl));
                        isSuccess = avaClient.PushCustomerIntoAvalara(customerDetails, out logFileMessage);
                        logMessage.AppendLine(logFileMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error In PushCustomerIntoAvalara Method: " + ex.Message.ToString());
            }
            finally
            {
            }
            return isSuccess;
        }

        /// <summary>
        /// Service SKU Eligible for a customer...
        /// </summary>
        /// <param name="aRequest"></param>
        /// <returns></returns>
        public SalesOrderResponse GetCustomerIsServiceSKUEligible(SalesOrderRequest aRequest)
        {
            SalesOrderResponse customerServiceSKUResponse = null;
            IAccountRepository salesDataAccess = null;
            aRequest.ThrowIfNull("aRequest");
            aRequest.SalesOrderEntity.ThrowIfNull("aRequest");
            try
            {
                customerServiceSKUResponse = new SalesOrderResponse();
                salesDataAccess = new ChemPoint.GP.Account.AccountDL.AccountDL(aRequest.ConnectionString);
                if(salesDataAccess.GetCustomerIsServiceSKUEligible(aRequest))
                    customerServiceSKUResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                else
                    customerServiceSKUResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Custom;

            }
            catch(Exception ex)
            {
                customerServiceSKUResponse.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                customerServiceSKUResponse.ErrorMessage = ex.Message.ToString().Trim();
            }

            return customerServiceSKUResponse;
        }

        
    }
}
