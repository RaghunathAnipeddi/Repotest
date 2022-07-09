using Chempoint.GP.Model.Interactions.AuditLog;
using Chempoint.GP.Model.Interactions.Purchases;
using ChemPoint.GP.BusinessContracts.AuditLog;
using ChemPoint.GP.CPServiceAuditLogBL;
//using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChemPoint.GP.XrmServices
{
    public class XrmService
    {
        private IAuditLogBusiness auditLogBusiness;

        public XrmService()
        {
            auditLogBusiness = new XrmAuditLogBusiness();
        }

        #region XRM-Publish

        /// <summary>
        /// Method to call update the notes in XRM.
        /// </summary>
        /// <param name="pickTicketNumber">pick ticket number</param>
        /// <param name="sopNumber">Sales Order Number</param>
        /// <param name="url">CRM URL</param>
        /// <param name="noteText">Note to be send</param>
        /// <returns>return status message</returns>
        public string PublishSalesOrderNotes(string sopNumber, string url, string userId, string notesText, string source, string companyName)
        {
            string result = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Dictionary<string, dynamic> @params = new Dictionary<string, dynamic>();
                    @params.Add("EntityName", "Notes");
                    @params.Add("Subject", "**** Entered By: " + userId + " @ " + DateTime.Now + " ****");
                    @params.Add("NoteText", notesText);
                    @params.Add("RelatedEntityName", "salesorder");
                    @params.Add("RelatedEntityId", sopNumber); //Pass Order number
                    @params.Add("RelatedEntityKeyName", "ordernumber");

                    HttpResponseMessage response = client.PostAsJsonAsync("api/GPAdapter/Publish/", @params).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        result = "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (result.Trim().Length > 0)
                    UpdateXrmIntegrationsAuditLog(sopNumber, "Notes Update", companyName, "Error in XRM : " + result, userId, source);
                else
                    UpdateXrmIntegrationsAuditLog(sopNumber, "Notes Update", companyName, "Status has been updated successfully.", userId, source);
            }
            return result;
        }

        /// <summary>
        /// Method to call update the status in XRM.
        /// </summary>
        /// <param name="sopNumber">Sales Order Number</param>
        /// <param name="url">CRM URL</param>
        /// <returns>return status message</returns>
        public string PublishSalesOrderStatus(string sopNumber, string companyName, string url, string userId, string source)
        {
            string result = string.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Dictionary<string, dynamic> @params = new Dictionary<string, dynamic>();
                    @params.Add("EntityId", sopNumber);
                    @params.Add("EntityName", "SalesOrderInformation");
                    @params.Add("CompanyId", companyName);
                    @params.Add("ModifiedBy", userId);

                    var task = new TaskFactory().StartNew(() =>
                    {
                        var response = client.PostAsJsonAsync("api/GPAdapter/Publish/", @params);
                        if (!response.Result.IsSuccessStatusCode)
                        {
                            result = "Failure";
                        }
                    });
                    task.Wait(3000);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (result.Trim().Length > 0)
                    UpdateXrmIntegrationsAuditLog(sopNumber, "Status Update", companyName, "Error in XRM : " + result, userId, source);
                else
                    UpdateXrmIntegrationsAuditLog(sopNumber, "Status Update", companyName, "Status has been updated successfully.", userId, source);
            }
        }

        #endregion XRM-Publish

        #region XRM-Audit

        /// <summary>
        /// Update audit log for XRM
        /// </summary>
        /// <param name="source">Specify source name</param>
        /// <param name="operation">Specify operation name</param>
        /// <param name="sopNumber">Specify order number</param>
        /// <param name="notes">Specify notes</param>
        private bool UpdateXrmIntegrationsAuditLog(string sopNumber, string operation, string companyName, string notes, string userId, string source)
        {
            bool result = false;
            try
            {
                XrmAuditLogRequest request = new XrmAuditLogRequest();
                request.AuditLogEntity.Source = source;
                request.AuditLogEntity.Operation = operation;
                request.AuditLogEntity.SourceDocumentId = sopNumber;
                request.AuditLogEntity.Notes = notes;
                request.AuditLogEntity.Company = (companyName.ToLower() == "chmpt" ? 1 : 2);
                request.AuditLogEntity.UserID = userId;
                request.AuditLogEntity.CreatedOn = DateTime.Now;
                auditLogBusiness.UpdateXrmIntegrationsAuditLog(request);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
            }
            return result;
        }
        
        #endregion XRM-Audit

        #region XRM-POActivity

        /// <summary>
        /// Publish Activity to XRM.
        /// </summary>
        /// <param name="createActivityDS"></param>
        /// <returns></returns>
        public string PublishPurchaseOrderActivity(PurchaseOrderRequest requestObj)
        {
            try
            {
                string status = string.Empty;
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(requestObj.CrmActivityConfigurationURL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Dictionary<string, dynamic> @params = new Dictionary<string, dynamic>();
                    @params.Add("Subject", requestObj.XRMIntegration.Subject);
                    @params.Add("Description", requestObj.XRMIntegration.Description);
                    @params.Add("Priority", requestObj.XRMIntegration.Priority);
                    @params.Add("DueDate", requestObj.XRMIntegration.DueDate);
                    @params.Add("RegardingEntityName", requestObj.XRMIntegration.RegardingEntityName); //PO creation.
                    @params.Add("RegardingEntityKeyName", requestObj.XRMIntegration.RegardingEntityKeyName);
                    @params.Add("RegardingEntityId", requestObj.XRMIntegration.RegardingEntityId);
                    @params.Add("OwnerEntityName", requestObj.XRMIntegration.OwnerEntityName);
                    @params.Add("OwnerEntityKeyName", requestObj.XRMIntegration.OwnerEntityKeyName); //Pass Order number
                    @params.Add("OwnerEntityId", requestObj.XRMIntegration.OwnerEntityId);

                    HttpResponseMessage response = client.PostAsJsonAsync("api/activity/CreateTask/", @params).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        status = "Failure";
                    }
                    return status;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }
        #endregion XRM-POActivity
    }
}
