using Chempoint.B2B;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using ChemPoint.GP.DataContracts.Sales;
using ChemPoint.GP.XrmServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Xsl;

namespace ChemPoint.GP.PickTicketBL
{
    public class PickTicketBusiness : IPickTicketBI
    {
        public PickTicketResponse PrintPickTicket(PickTicketRequest pickTicketRequest)
        {
            PickTicketResponse pickTicketResponse = null;

            int status = 0;
            string message = string.Empty;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                pickTicketResponse = new PickTicketResponse();
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "PickTicket Integration Started");
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Order Number : " + pickTicketRequest.SopNumber);

                ISalesOrderPickTicketRepository salesDataAccess = new ChemPoint.GP.SalesOrderDL.PickTicketDL(pickTicketRequest.ConnectionString);
                if (pickTicketRequest.CompanyID == 1)
                {
                    if (!string.IsNullOrEmpty(pickTicketRequest.SopNumber))
                    {
                        //declaration and assignment
                        bool isPTSendToWH = false, isPTSendToCarrier = false, isWHEdiIntegrated = false;

                        string invoiceNo = pickTicketRequest.SopNumber;
                        string requestType = pickTicketRequest.RequestType;
                        string styleSheetPath = pickTicketRequest.StyleSheetPath;
                        string user = pickTicketRequest.UserID;

                        //Call DB to get the sales order details & its eligibility
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling DB to get the details of the sales order.");

                        DataSet salesOrderDetails = (DataSet)salesDataAccess.GetSalesOrderDetailsForPT(pickTicketRequest.SopNumber, pickTicketRequest.SopType);

                        if (salesOrderDetails != null && salesOrderDetails.Tables.Count > 0 && salesOrderDetails.Tables[0].Rows.Count > 0)
                        {
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsEdiWarehouse"].ToString()))
                                isWHEdiIntegrated = true;

                            //check if order eligible
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsIntegratedWarehouse"].ToString()) &&
                                Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsWHAllowedToInsert"].ToString()))
                                isPTSendToWH = true;

                            // check if carrier eligible
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsIntegratedCarrier"].ToString()) &&
                                Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsCarrierAllowedToInsert"].ToString()))
                                isPTSendToCarrier = true;

                            if (!isWHEdiIntegrated && !isPTSendToWH && !isPTSendToCarrier)
                            {
                                status = -1;
                                message = "Integration does not applicable to send pickTicket if AllowToInsert is set as false";
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Integration does not applicable to send pickTicket if AllowToInsert is set as false");
                            }
                            else
                            {
                                string warehouseID = salesOrderDetails.Tables[0].Rows[0]["WarehouseId"].ToString().Trim();
                                bool isCarrierEnabled = isPTSendToCarrier;
                                string carrierID = salesOrderDetails.Tables[0].Rows[0]["Carrier"].ToString().Trim();

                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Warehouse ID : " + warehouseID);
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Carrier ID : " + carrierID);

                                if (status == 0 && isWHEdiIntegrated)
                                {
                                    if (String.IsNullOrEmpty(pickTicketRequest.WarehouseID))
                                    {
                                        pickTicketRequest.WarehouseID = warehouseID;
                                    }
                                    PickTicketResponse pickTicketEdiResponse = SendPickTicketToEdiWarehouse(pickTicketRequest);
                                    if (pickTicketEdiResponse != null)
                                    {
                                        if (pickTicketEdiResponse.Status == ResponseStatus.Success)
                                        {
                                            status = 0;
                                            message = pickTicketEdiResponse.Message;
                                        }
                                        else
                                        {
                                            status = -1;
                                            if (!string.IsNullOrWhiteSpace(pickTicketEdiResponse.Message))
                                            {
                                                message = pickTicketEdiResponse.Message;
                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Unknown error occurred and unable to process pick ticket due to " + pickTicketEdiResponse.Message + ". Please contact IT.");
                                            }
                                            else
                                            {
                                                message = "Unknown error occurred and unable to process pick ticket. Please contact IT.";
                                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Unknown error occurred and unable to process pick ticket. Please contact IT.");
                                            }
                                        }
                                    }
                                }
                                if (status == 0 && (isPTSendToWH || isPTSendToCarrier))
                                {
                                    string str1 = string.Empty;
                                    string strResultWarehouse = string.Empty;
                                    string strResultCarrier = string.Empty;

                                    try
                                    {
                                        XmlDocument requestXml = new XmlDocument();

                                        //Get order details
                                        DataSet orderDetails = new DataSet("SOPDetails");
                                        orderDetails.Tables.Add(salesOrderDetails.Tables[1].Copy());
                                        orderDetails.Tables[0].TableName = "SOP";
                                        string orderXml = orderDetails.GetXml();

                                        //Convert order details to WH format
                                        string shipOrderXml = TransToShipOrder(orderXml, styleSheetPath);
                                        requestXml.LoadXml(shipOrderXml);

                                        string message1 = string.Empty;
                                        if (IsValidRequest(requestXml, salesOrderDetails.Tables[2], ref message1))
                                        {
                                            string request = requestXml.SelectSingleNode("/ShipOrder").OuterXml;

                                            string assemblyWarehouse = salesOrderDetails.Tables[0].Rows[0]["AssemblyWarehouse"].ToString().Trim();
                                            string assemblyCarrier = salesOrderDetails.Tables[0].Rows[0]["AssemblyCarrier"].ToString().Trim();

                                            if (isPTSendToWH)
                                            {
                                                bool isAllowToProcess = false;
                                                if (requestType.ToUpper().Equals("DELETE"))
                                                    isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsWHAllowedToDelete"].ToString());
                                                else if (requestType.ToUpper().Equals("UPDATE"))
                                                    isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsWHAllowedToUpdate"].ToString());
                                                else if (requestType.ToUpper().Equals("INSERT"))
                                                    isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsWHAllowedToInsert"].ToString());

                                                strResultWarehouse = ProcessRequestForWarehouse(invoiceNo,
                                                    warehouseID,
                                                    carrierID,
                                                    request,
                                                    isAllowToProcess,
                                                    user,
                                                    requestType,
                                                    assemblyWarehouse,
                                                    salesOrderDetails.Tables[0].Rows[0]["WarehouseMsgOut"].ToString(),
                                                    ref isCarrierEnabled,
                                                    ref message);
                                            }
                                            else
                                            {
                                                strResultWarehouse = "<ShipOrderAck Version=\"2.0\"><Order></Order></ShipOrderAck>";
                                            }

                                            if (isPTSendToCarrier)
                                            {
                                                if (isCarrierEnabled && this.GetStatus(strResultWarehouse).Equals(0))
                                                {
                                                    bool isAllowToProcess = false;
                                                    if (requestType.ToUpper().Equals("DELETE"))
                                                        isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsCarrierAllowedToDelete"].ToString());
                                                    else if (requestType.ToUpper().Equals("UPDATE"))
                                                        isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsCarrierAllowedToUpdate"].ToString());
                                                    else if (requestType.ToUpper().Equals("INSERT"))
                                                        isAllowToProcess = Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsCarrierAllowedToInsert"].ToString());

                                                    strResultCarrier = ProcessRequestForCarrier(invoiceNo,
                                                        warehouseID,
                                                        carrierID,
                                                        request,
                                                        isAllowToProcess,
                                                        user,
                                                        requestType,
                                                        assemblyCarrier,
                                                        salesOrderDetails.Tables[0].Rows[0]["CarrierMsgOut"].ToString(),
                                                        Convert.ToInt16(salesOrderDetails.Tables[0].Rows[0]["CarrierStatus"]),
                                                        salesOrderDetails.Tables[0].Rows[0]["CarrierTransType"].ToString().Trim(),
                                                        ref isCarrierEnabled,
                                                        ref message);
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(strResultWarehouse))
                                                str1 = strResultWarehouse;
                                            else
                                                str1 = strResultCarrier;

                                            if (!string.IsNullOrEmpty(str1))
                                            {
                                                requestXml.LoadXml(str1);
                                                XmlNodeList elementsByTagName = requestXml.GetElementsByTagName("Fault");
                                                if (elementsByTagName.Count > 0)
                                                {
                                                    string innerText = elementsByTagName[0].SelectSingleNode("Message").InnerText;
                                                    status = int.Parse(elementsByTagName[0].SelectSingleNode("Code").InnerText);
                                                    int num = 0;
                                                    if (!string.IsNullOrEmpty(message))
                                                        num = message.Length;
                                                    int length = innerText.Length;
                                                    if (num + length <= (int)byte.MaxValue)
                                                        message = message + "(" + innerText + ")";
                                                }
                                                else
                                                    status = 0;
                                            }
                                        }
                                        else
                                        {
                                            message = message1;
                                            status = -1;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        string str2 = ex.Message + ". Source: " + ex.Source;
                                        try
                                        {
                                            if (status == 0)
                                                status = -1;
                                            if (string.IsNullOrEmpty(str1))
                                                str1 = str2;

                                            message = str1;
                                        }
                                        catch
                                        {
                                            message = str2;
                                            status = -1;
                                        }
                                    }
                                }
                                //update status
                                if (status == 0)
                                {
                                    if (requestType.ToUpper().Equals("INSERT") || requestType.ToUpper().Equals("UPDATE"))
                                    {
                                        salesDataAccess.UpdateSalesDocumentStatus(pickTicketRequest.SopNumber, pickTicketRequest.CompanyID);
                                    }

                                    //PUBLISH order Notes to XRM
                                    string xRMNotesResult = new XrmService().PublishSalesOrderNotes(pickTicketRequest.OrigNumber, pickTicketRequest.XrmServiceUrl, pickTicketRequest.UserID, "Print Pick Ticket: " + pickTicketRequest.SopNumber, (pickTicketRequest.UserID == "Auto PT" ? "AutoTransfer" : "GP"), "chmpt");
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - -- XRM order notes publish result - " + xRMNotesResult);

                                    //PUBLISH order status to XRM
                                    string xrmStatusResult = new XrmService().PublishSalesOrderStatus(pickTicketRequest.OrigNumber,
                                                                                  (pickTicketRequest.CompanyID == 1 ? "chmpt" : "cpeur"), pickTicketRequest.XrmServiceUrl, pickTicketRequest.UserID, (pickTicketRequest.UserID == "Auto PT" ? "AutoTransferToFO" : "GP"));
                                    logMessage.AppendLine(DateTime.Now.ToString() + " - -- XRM order status publish result - " + xrmStatusResult);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Data does not found");
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Invoice should not be blank");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(pickTicketRequest.SopNumber))
                    {
                        PickTicketResponse pickTicketEdiResponse = SendPickTicketToEdiWarehouse(pickTicketRequest);
                        if (pickTicketEdiResponse != null)
                        {
                            if (pickTicketEdiResponse.Status == ResponseStatus.Success)
                            {
                                status = 0;
                                message = pickTicketEdiResponse.Message;
                            }
                            else
                            {
                                status = -1;
                                if (!string.IsNullOrWhiteSpace(pickTicketEdiResponse.Message))
                                {
                                    message = pickTicketEdiResponse.Message;
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Unknown error occurred and unable to process pick ticket due to " + pickTicketEdiResponse.Message + ". Please contact IT.");
                                }
                                else
                                {
                                    message = "Unknown error occurred and unable to process pick ticket. Please contact IT.";
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Unknown error occurred and unable to process pick ticket. Please contact IT.");
                                }
                            }
                            //update status
                            if (status == 0)
                            {
                                if (pickTicketRequest.RequestType.ToUpper().Equals("INSERT") || pickTicketRequest.RequestType.ToUpper().Equals("UPDATE"))
                                {
                                    salesDataAccess.UpdateSalesDocumentStatus(pickTicketRequest.SopNumber, pickTicketRequest.CompanyID);
                                }

                                //PUBLISH order Notes to XRM
                                string xRMNotesResult = new XrmService().PublishSalesOrderNotes(pickTicketRequest.OrigNumber, pickTicketRequest.XrmServiceUrl, pickTicketRequest.UserID, "Print Pick Ticket: " + pickTicketRequest.SopNumber, (pickTicketRequest.UserID == "Auto PT" ? "AutoTransfer" : "GP"), "chmpt");
                                logMessage.AppendLine(DateTime.Now.ToString() + " - -- XRM order notes publish result - " + xRMNotesResult);

                                //PUBLISH order status to XRM
                                string xrmStatusResult = new XrmService().PublishSalesOrderStatus(pickTicketRequest.OrigNumber,
                                                                              (pickTicketRequest.CompanyID == 1 ? "Chmpt" : "Cpeur"), pickTicketRequest.XrmServiceUrl, pickTicketRequest.UserID, (pickTicketRequest.UserID == "Auto PT" ? "AutoTransferToFO" : "GP"));
                                logMessage.AppendLine(DateTime.Now.ToString() + " - -- XRM order status publish result - " + xrmStatusResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = -1;
                message = "Error: " + ex.Message;
            }
            finally
            {
                pickTicketResponse.Status = status.Equals(-1) ? ResponseStatus.Error : ResponseStatus.Success;
                pickTicketResponse.Message = message;

                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Message : " + pickTicketResponse.Message);


                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "PickTicket Integration Ended");

                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), pickTicketRequest.LoggingPath, pickTicketRequest.LoggingFileName);
            }
            return pickTicketResponse;
        }

        private PickTicketResponse SendPickTicketToEdiWarehouse(PickTicketRequest pickTicketRequest)
        {
            PickTicketResponse pickTicketEdiResponse = null;

            try
            {
                pickTicketEdiResponse = new PickTicketResponse();
                using (HttpClient client = new HttpClient())
                {
                    string configUrl = pickTicketRequest.WarehouseEdiServiceUrl;
                    client.BaseAddress = new Uri(configUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    string url = "api/Shipment/SendShippingOrder/?InvoiceNumber={0}&InvoiceType={1}&WarehouseId={2}&CompanyName={3}&UserName={4}&OperationStatus={5}";
                    url = string.Format(url, new object[]
                    {
                        pickTicketRequest.SopNumber, pickTicketRequest.SopType,
                        pickTicketRequest.WarehouseID, pickTicketRequest.CompanyName, pickTicketRequest.UserID, pickTicketRequest.OperationStatus
                    });
                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string mdnResponse = response.Content.ReadAsAsync<string>().Result;
                        if (!string.IsNullOrWhiteSpace(mdnResponse))
                        {
                            pickTicketEdiResponse.Message = mdnResponse;
                            pickTicketEdiResponse.Status = ResponseStatus.Success;
                        }
                        else
                        {
                            pickTicketEdiResponse.Message = "Empty MDNResponse";
                            pickTicketEdiResponse.Status = ResponseStatus.Unknown;
                        }
                    }
                    else
                    {
                        Chempoint.GP.Model.Interactions.Sales.Error error = response.Content.ReadAsAsync<Chempoint.GP.Model.Interactions.Sales.Error>().Result;
                        pickTicketEdiResponse.Message = error.Message;
                        pickTicketEdiResponse.Status = (response.StatusCode == System.Net.HttpStatusCode.NotAcceptable) ? ResponseStatus.Custom : ResponseStatus.Error;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
            return pickTicketEdiResponse;
        }

        public PickTicketResponse GetOrderPickTicketEligibleDetails(PickTicketRequest pickTicketRequest)
        {
            PickTicketResponse pickTicketResponse = null;

            int status = 0;
            string message = string.Empty;
            StringBuilder logMessage = new StringBuilder();
            try
            {
                pickTicketResponse = new PickTicketResponse();
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "PickTicket GetOrderPickTicketEligibleDetails Integration Started");
                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Invoice Number : " + pickTicketRequest.SopNumber);

                if (pickTicketRequest.CompanyID == 1)
                {
                    if (!string.IsNullOrEmpty(pickTicketRequest.SopNumber))
                    {
                        //Call DB to get the sales order details & its eligibility
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Calling DB to get the details of the sales order.");
                        ISalesOrderPickTicketRepository salesDataAccess = new ChemPoint.GP.SalesOrderDL.PickTicketDL(pickTicketRequest.ConnectionString);
                        DataSet salesOrderDetails = (DataSet)salesDataAccess.GetSalesOrderDetailsForPT(pickTicketRequest.SopNumber, pickTicketRequest.SopType);

                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Get Sales order Details for PT.");

                        if (salesOrderDetails != null && salesOrderDetails.Tables.Count > 0 && salesOrderDetails.Tables[0].Rows.Count > 0)
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + (salesOrderDetails.Tables[0].Rows[0]["IsEdiWarehouse"].ToString()));
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsEdiWarehouse"].ToString()))
                                pickTicketResponse.IsEdiIntegratedWarehouse = true;

                            //check if order eligible
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsIntegratedWarehouse"].ToString()) &&
                                Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsWHAllowedToInsert"].ToString()))
                                pickTicketResponse.IsXmlIntegratedWarehouse = true;

                            // check if carrier eligible
                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsIntegratedCarrier"].ToString()) &&
                                Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsCarrierAllowedToInsert"].ToString()))
                                pickTicketResponse.IsIntegratedCarrier = true;

                            if (Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsPTSentToWH"].ToString()) || Convert.ToBoolean(salesOrderDetails.Tables[0].Rows[0]["IsPTSentToCarrier"].ToString()))
                            {
                                pickTicketResponse.IsXmlPTSendBefore = true;
                            }

                            pickTicketResponse.WarehouseId = salesOrderDetails.Tables[0].Rows[0]["WarehouseId"].ToString().Trim();
                            pickTicketResponse.WarehouseName = salesOrderDetails.Tables[0].Rows[0]["WarehouseDescription"].ToString().Trim();
                            pickTicketResponse.CarrierId = salesOrderDetails.Tables[0].Rows[0]["Carrier"].ToString().Trim();
                            if (pickTicketResponse.IsEdiIntegratedWarehouse)
                            {
                                logMessage.AppendLine(System.DateTime.Now.ToString() + " WarehouseId-- " + pickTicketResponse.WarehouseId);
                                logMessage.AppendLine(System.DateTime.Now.ToString() + " WarehouseEdiServiceUrl-- " + (pickTicketRequest.WarehouseEdiServiceUrl));
                                logMessage.AppendLine(System.DateTime.Now.ToString() + " SopNumber-- " + (pickTicketRequest.SopNumber));
                                logMessage.AppendLine(System.DateTime.Now.ToString() + " SopType-- " + (pickTicketRequest.SopType));
                                logMessage.AppendLine(System.DateTime.Now.ToString() + " CompanyName-- " + (pickTicketRequest.CompanyName));

                                PickTicketResponse ediPTResponse = GetPickTicketStatusOfEdiWarehouse(pickTicketRequest);
                                
                                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "GetPickTicketStatusOfEdiWarehouse Success");
                                if (string.IsNullOrEmpty(ediPTResponse.Message))
                                {
                                    pickTicketResponse.IsEdiIntegratedWarehouse = ediPTResponse.IsEdiIntegratedWarehouse;
                                    pickTicketResponse.EdiPTSendOperation = ediPTResponse.EdiPTSendOperation;
                                    pickTicketResponse.EdiPTSendErrorMessage = ediPTResponse.EdiPTSendErrorMessage;
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + pickTicketResponse.EdiPTSendErrorMessage);
                                }
                                else
                                {
                                    pickTicketResponse.EdiPTSendErrorMessage = ediPTResponse.Message;
                                    logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while calling EDI service : " + ediPTResponse.Message);
                                }
                            }
                        }
                        else
                        {
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Data does not found");
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Invoice should not be blank");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(pickTicketRequest.SopNumber))
                    {
                        PickTicketResponse ediPTResponse = GetPickTicketStatusOfEdiWarehouse(pickTicketRequest);

                        if (string.IsNullOrEmpty(ediPTResponse.Message))
                        {
                            pickTicketResponse.IsEdiIntegratedWarehouse = ediPTResponse.IsEdiIntegratedWarehouse;
                            pickTicketResponse.EdiPTSendOperation = ediPTResponse.EdiPTSendOperation.Trim();
                            pickTicketResponse.EdiPTSendErrorMessage = ediPTResponse.EdiPTSendErrorMessage;
                            pickTicketResponse.WarehouseId = ediPTResponse.WarehouseId.Trim();
                            pickTicketResponse.WarehouseName = ediPTResponse.WarehouseName.Trim();
                            pickTicketResponse.CarrierId = ediPTResponse.CarrierId.Trim();
                        }
                        else
                        {
                            pickTicketResponse.EdiPTSendErrorMessage = ediPTResponse.Message;
                            logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Error while calling EDI service : " + ediPTResponse.Message);
                        }
                    }
                    else
                    {
                        logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Invoice should not be blank");
                    }
                }
                pickTicketResponse.Status = ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                status = -1;
                message = "Error: " + ex.Message;
            }
            finally
            {
                pickTicketResponse.Status = status.Equals(-1) ? ResponseStatus.Error : ResponseStatus.Success;
                pickTicketResponse.Message = message;

                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "Message : " + pickTicketResponse.Message);


                logMessage.AppendLine(System.DateTime.Now.ToString() + "-- " + "PickTicket Integration Ended");

                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), pickTicketRequest.LoggingPath, pickTicketRequest.LoggingFileName);
            }
            return pickTicketResponse;
        }

        private PickTicketResponse GetPickTicketStatusOfEdiWarehouse(PickTicketRequest pickTicketRequest)
        {
            PickTicketResponse pickTicketEdiResponse = null;

            try
            {
                pickTicketEdiResponse = new PickTicketResponse();
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(pickTicketRequest.WarehouseEdiServiceUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    List<SalesDocumentStatus> statusList = new List<SalesDocumentStatus>();
                    statusList.Add(new SalesDocumentStatus() { SopNumber = pickTicketRequest.SopNumber, SopType = pickTicketRequest.SopType });
                    JArray paramList = new JArray();
                    paramList.Add(JsonConvert.SerializeObject(statusList));
                    paramList.Add(JsonConvert.SerializeObject(pickTicketRequest.CompanyName));
                    HttpResponseMessage response = client.PostAsJsonAsync("api/Shipment/GetSalesDocumentStatusByInvoice", paramList).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        List<SalesDocumentStatus> result = response.Content.ReadAsAsync<List<SalesDocumentStatus>>().Result;
                        if (result != null && result.Count > 0)
                        {
                            pickTicketEdiResponse.EdiPTSendOperation = result[0].DocumentOperationStatus.ToString();
                            pickTicketEdiResponse.EdiPTSendErrorMessage = result[0].ValidationMessage;
                            pickTicketEdiResponse.IsEdiIntegratedWarehouse = result[0].IsIntegrated;
                            pickTicketEdiResponse.WarehouseId = result[0].WarehouseId;
                            pickTicketEdiResponse.WarehouseName = result[0].WarehouseName;
                            pickTicketEdiResponse.CarrierId = result[0].CarrierId;
                        }
                    }
                    else
                    {
                        Error error = response.Content.ReadAsAsync<Error>().Result;
                        pickTicketEdiResponse.Message = error.Message;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
            return pickTicketEdiResponse;
        }

        private string ProcessRequestForWarehouse(string invoiceNo, string warehouseId, string carrierId, string request, bool isAllowToProcess, string requestor, string requestType, string assemblyDll, string warehouseMsgOut, ref bool isCarrierEnabled, ref string message)
        {
            string strResultWarehouse = string.Empty;
            bool flag1 = false;
            bool flag2 = false;

            try
            {
                if (requestType.ToUpper().Equals("DELETE"))
                {
                    if (isAllowToProcess)
                    {
                        string str8 = warehouseMsgOut;
                        str8 = !str8.Equals("") ? str8.Replace("Insert", requestType) : "";
                        if (str8.Trim().Length > 0)
                        {
                            strResultWarehouse = SendPickTicketToXml(invoiceNo,
                                warehouseId,
                                "",
                                str8,
                                requestor,
                                requestType,
                                assemblyDll); //assembly path

                            TransLog.LogTrans(warehouseId, invoiceNo, invoiceNo, requestType, str8, strResultWarehouse, requestor, this.GetStatus(strResultWarehouse));
                        }
                        else
                        {
                            strResultWarehouse = "<ShipOrderAck Version=\"2.0\"><Order><OrderNo>" + invoiceNo + "</OrderNo><OrderStatus>Delete</OrderStatus></Order></ShipOrderAck>";
                            isCarrierEnabled = false;
                        }
                    }
                    else
                    {
                        strResultWarehouse = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Delete not Supported for Warehouse. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                }
                else if (requestType.ToUpper().Equals("UPDATE"))
                {
                    if (isAllowToProcess)
                    {
                        requestType = "Delete";
                        string str8 = warehouseMsgOut;
                        str8 = !str8.Equals("") ? str8.Replace("Insert", requestType) : "";
                        if (str8.Trim().Length > 0)
                        {
                            strResultWarehouse = SendPickTicketToXml(invoiceNo,
                                warehouseId,
                                "",
                                str8,
                                requestor,
                                requestType,
                                assemblyDll); //assembly path
                        }
                        else
                        {
                            strResultWarehouse = "<ShipOrderAck Version=\"2.0\"><Order><OrderNo>" + invoiceNo + "</OrderNo><OrderStatus>Delete</OrderStatus></Order></ShipOrderAck>";
                        }
                        TransLog.LogTrans(warehouseId, invoiceNo, invoiceNo, requestType, str8, strResultWarehouse, requestor, this.GetStatus(strResultWarehouse));

                        if (this.GetStatus(strResultWarehouse) == 0)
                        {
                            requestType = "Insert";
                            string str9 = request.Replace("Update", requestType);

                            strResultWarehouse = SendPickTicketToXml(invoiceNo,
                                warehouseId,
                                "",
                                str9,
                                requestor,
                                requestType,
                                assemblyDll); //assembly path

                            if (this.GetStatus(strResultWarehouse) != 0)
                            {
                                flag2 = true;
                            }
                            TransLog.LogTrans(warehouseId, invoiceNo, invoiceNo, requestType, str9, strResultWarehouse, requestor, this.GetStatus(strResultWarehouse));
                        }
                        else
                        {
                            flag1 = true;
                        }
                    }
                    else
                    {
                        strResultWarehouse = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Update not Supported for Warehouse. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                    requestType = "Update";
                }
                else if (requestType.ToUpper().Equals("INSERT"))
                {
                    if (isAllowToProcess)
                    {
                        strResultWarehouse = SendPickTicketToXml(invoiceNo,
                            warehouseId,
                            "",
                            request,
                            requestor,
                            requestType,
                            assemblyDll); //assembly path

                        TransLog.LogTrans(warehouseId, invoiceNo, invoiceNo, requestType, request, strResultWarehouse, requestor, this.GetStatus(strResultWarehouse));
                    }
                    else
                    {
                        strResultWarehouse = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Insert not Supported for Warehouse. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                }
                if (isCarrierEnabled && this.GetStatus(strResultWarehouse) != 0)
                {
                    TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, request, strResultWarehouse, requestor, this.GetStatus(strResultWarehouse));
                }
            }
            catch (Exception ex)
            {
                return "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>" + ex.Message + "</Message></Fault></Header></ShipOrderAck>";
            }
            finally
            {
                if (!string.IsNullOrEmpty(strResultWarehouse))
                {
                    if (!requestType.ToLower().Equals("update"))
                    {
                        message = GetMessage(invoiceNo, warehouseId, carrierId, requestType, isCarrierEnabled, GetStatus(strResultWarehouse), -1);
                    }
                    else
                    {
                        if (!flag1)
                        {
                            if (!flag2)
                            {
                                message = GetMessage(invoiceNo, warehouseId, carrierId, requestType, isCarrierEnabled, GetStatus(strResultWarehouse), -1);
                            }
                            else
                            {
                                message = GetMessage(invoiceNo, warehouseId, carrierId, requestType, isCarrierEnabled, -1, -1);
                            }
                        }
                        else
                        {
                            message = GetMessage(invoiceNo, warehouseId, carrierId, requestType, isCarrierEnabled, -2, -1);
                        }
                    }
                }
            }
            return strResultWarehouse;
        }

        private string ProcessRequestForCarrier(string invoiceNo, string warehouseId, string carrierId, string request, bool isAllowToProcess, string requestor, string requestType, string assemblyDll, string carrierMsgOut, int carrierStatus, string carrierTransType, ref bool isCarrierEnabled, ref string message)
        {
            string strResultCarrier = string.Empty;

            try
            {
                if (requestType.ToUpper().Equals("DELETE"))
                {
                    if (isAllowToProcess)
                    {
                        string str8 = carrierMsgOut;
                        str8 = !str8.Equals("") ? str8.Replace("Insert", requestType) : "";
                        str8 = !str8.Equals("") ? str8.Replace("Update", requestType) : "";
                        if (str8.Trim().Length > 0)
                        {
                            strResultCarrier = SendPickTicketToXml(invoiceNo,
                                warehouseId,
                                carrierId,
                                str8,
                                requestor,
                                requestType,
                                assemblyDll); //assembly path

                            TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, request, strResultCarrier, requestor, this.GetStatus(strResultCarrier));
                        }
                        else
                        {
                            strResultCarrier = "<ShipOrderAck Version=\"2.0\"><Order><OrderNo>" + invoiceNo + "</OrderNo><OrderStatus>Delete</OrderStatus></Order></ShipOrderAck>";
                        }
                    }
                    else
                    {
                        strResultCarrier = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Delete not Supported for freight. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                }
                else if (requestType.ToUpper().Equals("UPDATE"))
                {
                    if (isAllowToProcess)
                    {
                        if (carrierStatus == 0)
                        {
                            strResultCarrier = SendPickTicketToXml(invoiceNo,
                                warehouseId,
                                carrierId,
                                request,
                                requestor,
                                requestType,
                                assemblyDll); //assembly path

                            TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, request, strResultCarrier, requestor, this.GetStatus(strResultCarrier));
                        }
                        else
                        {
                            if (carrierTransType.ToUpper().Equals("INSERT") || carrierTransType.ToUpper().Equals("DELETE"))
                            {
                                requestType = "Insert";
                                string str10 = request.Replace("Update", requestType);
                                strResultCarrier = SendPickTicketToXml(invoiceNo,
                                    warehouseId,
                                    carrierId,
                                    str10,
                                    requestor,
                                    requestType,
                                    assemblyDll); //assembly path
                                TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, str10, strResultCarrier, requestor, this.GetStatus(strResultCarrier));
                            }
                            else if (carrierTransType.ToUpper().Equals("UPDATE"))
                            {
                                strResultCarrier = SendPickTicketToXml(invoiceNo,
                                    warehouseId,
                                    carrierId,
                                    request,
                                    requestor,
                                    requestType,
                                    assemblyDll); //assembly path
                                TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, request, strResultCarrier, requestor, this.GetStatus(strResultCarrier));
                            }
                        }
                    }
                    else
                    {
                        strResultCarrier = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Update not Supported for freight. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                    requestType = "Update";
                }
                else if (requestType.ToUpper().Equals("INSERT"))
                {
                    if (isAllowToProcess)
                    {
                        strResultCarrier = SendPickTicketToXml(invoiceNo,
                            warehouseId,
                            carrierId,
                            request,
                            requestor,
                            requestType,
                            assemblyDll); //assembly path

                        TransLog.LogTransForFreight(carrierId, warehouseId, invoiceNo, invoiceNo, requestType, request, strResultCarrier, requestor, this.GetStatus(strResultCarrier));
                    }
                    else
                    {
                        strResultCarrier = "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>Insert not Supported for freight. Contact IT!</Message></Fault></Header></ShipOrderAck>";
                    }
                }
            }
            catch (Exception ex)
            {
                return "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>" + ex.Message + "</Message></Fault></Header></ShipOrderAck>";
            }
            finally
            {
                if (!string.IsNullOrEmpty(strResultCarrier))
                {
                    message = GetMessage(invoiceNo, warehouseId, carrierId, requestType, isCarrierEnabled, -1, GetStatus(strResultCarrier));
                }
            }
            return strResultCarrier;
        }

        private bool IsValidRequest(XmlDocument requestXml, DataTable warehouseInfoTable, ref string message)
        {
            bool flag = true;
            message = string.Empty;
            Hashtable warehouseInfo = new Hashtable();

            foreach (DataRow dataRow in (InternalDataCollectionBase)warehouseInfoTable.Rows)
                warehouseInfo.Add(dataRow["Name"], dataRow["Value"]);

            if (requestXml == null)
            {
                message = warehouseInfo[(object)"InvalidRequest"].ToString().Trim();
                flag = false;
            }
            else
            {
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("CarrierCode"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgCarrierRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("FreightTerm"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgFreightTypeRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ShipToAddress1"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgShippingAddress1Required"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ShipToPostalCode"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgShippingZipRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ShipToCity"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgShippingCityRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ShipToStateProvince"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgShippingStateRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ShipToCompanyName"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgShippingFirstNameRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("ToPartnerID"))
                {
                    if (string.IsNullOrEmpty(xmlNode.InnerText))
                    {
                        message = warehouseInfo[(object)"ErrMsgPartnerInfoOrPartnerIdentiRequired"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                foreach (XmlNode xmlNode in requestXml.GetElementsByTagName("RequestedShipDate"))
                {
                    DateTime result = new DateTime();
                    if (DateTime.TryParse(xmlNode.InnerText, out result))
                    {
                        message = warehouseInfo[(object)"ErrMsgRequestedShipDateFormat"].ToString().Trim();
                        flag = false;
                        break;
                    }
                }
                if (requestXml.InnerXml.IndexOf("19000101") > 0)
                {
                    message = warehouseInfo[(object)"ErrMsgRequestedShipDateRequired"].ToString().Trim();
                    flag = false;
                }
            }
            return flag;
        }

        private string SendPickTicketToXml(string invoiceNo, string warehouseId, string carrier, string request, string requestor, string requestType, string assemblyDll)
        {
            string isResult = string.Empty;
            try
            {
                XmlDocument xmlDocumentPickTicket = new XmlDocument();
                xmlDocumentPickTicket.LoadXml(request);

                PickTicketProcessFactory pickTicketProcessFactory = new PickTicketProcessFactory();
                IWarehouseIntegrator warehouseIntegrator = null;
                switch (assemblyDll)
                {
                    case "Chempoint.B2B.CHR":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.CHR);
                        break;
                    case "Chempoint.B2B.DHL":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.DHL);
                        break;
                    case "Chempoint.B2B.GA":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.GA);
                        break;
                    case "Chempoint.B2B.GL":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.GL);
                        break;
                    case "Chempoint.B2B.Jacobson":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.Jacobson);
                        break;
                    case "Chempoint.B2B.NA":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.NA);
                        break;
                    case "Chempoint.B2B.SouthernBonded":
                        warehouseIntegrator = pickTicketProcessFactory.Process(PickTicketOperationType.SouthernBonded);
                        break;
                    default:
                        warehouseIntegrator = null;
                        break;
                }

                if (warehouseIntegrator == null)
                {
                    return "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>" + assemblyDll + " is not implemented.</Message></Fault></Header></ShipOrderAck>";
                }
                else
                {
                    ThreadWrapper[] threadWrapperArray = new ThreadWrapper[1];

                    threadWrapperArray[0] = new ThreadWrapper();
                    threadWrapperArray[0].Args = new XsltArgumentList();
                    threadWrapperArray[0].Args.AddParam("WarehouseID", "", (object)warehouseId);
                    if (!carrier.Equals(""))
                        threadWrapperArray[0].Args.AddParam("CarrierID", "", (object)carrier);
                    threadWrapperArray[0].Args.AddParam("Requestor", "", (object)requestor);
                    threadWrapperArray[0].Args.AddParam("RequestType", "", (object)requestType);
                    threadWrapperArray[0].Args.AddParam("InvoiceNo", "", (object)invoiceNo);
                    threadWrapperArray[0].RequestMessage = xmlDocumentPickTicket.InnerXml;
                    threadWrapperArray[0].Integrator = warehouseIntegrator;
                    threadWrapperArray[0].Start();
                    threadWrapperArray[0].Join();
                    isResult = threadWrapperArray[0].ResponseMessage;
                }
                return isResult;
            }
            catch (Exception ex)
            {
                return "<ShipOrderAck><Header Version=\"2.0\"><Fault><Code>-1</Code><Message>" + ex.Message + "</Message></Fault></Header></ShipOrderAck>";
            }
        }

        private string TransToShipOrder(string strXml, string styleSheetPath)
        {
            // Local variables.
            string transformedXml = string.Empty;
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();
            //Creating Argument List Object
            XsltArgumentList xsltArgs = new XsltArgumentList();

            //Loading  XML
            xmlDoc.LoadXml(strXml);
            xslTrans.Load(styleSheetPath);

            //Creating StringWriter Object
            StringWriter strWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgs, strWriter);
            // Set the transformed xml.
            transformedXml = strWriter.ToString().Trim();
            // Dispose the objects.                                                
            strWriter.Dispose();

            // Return the transformed xml to the caller
            return transformedXml;
        }

        private int GetStatus(string resXml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(resXml);
            return xmlDocument.GetElementsByTagName("Fault", "").Count == 0 ? 0 : -1;
        }

        private string GetMessage(string invoiceNo, string warehouseId, string carrierId, string requestType, bool isCarrierEnabled, int warehouseStatus, int freightstatus)
        {
            string messagebyStatus = this.GetMessagebyStatus(requestType, warehouseStatus, isCarrierEnabled, freightstatus);
            return Utilities.GetMessageFromDB(invoiceNo, messagebyStatus, isCarrierEnabled, carrierId, warehouseId);
        }

        private string GetMessagebyStatus(string requestType, int warehouseStatus, bool isCarrierEnabled, int carrierStatus)
        {
            return Utilities.GetResponseMessage(requestType, warehouseStatus, isCarrierEnabled, carrierStatus);
        }
    }

    public class ThreadWrapper
    {
        public IWarehouseIntegrator Integrator;
        public XsltArgumentList Args;
        public int OptionIndex;
        public string RequestMessage;
        public string ResponseMessage;
        private Thread thread;

        public ThreadWrapper()
        {
            this.thread = new Thread(new ThreadStart(this.ProcessRequest));
        }

        public void Start()
        {
            this.thread.Start();
        }

        public void Join()
        {
            this.thread.Join();
        }

        public void ProcessRequest()
        {
            this.ResponseMessage = this.SendRequest(this.RequestMessage, this.Args, this.Integrator);
        }

        private string SendRequest(string request, XsltArgumentList args, IWarehouseIntegrator Integrator)
        {
            try
            {
                int timeoutInMilSeconds = 30000;
                return Integrator.SubmitRequest(request, args, timeoutInMilSeconds);
            }
            catch (Exception ex)
            {
                Exception exception = ex;
                try
                {
                    return ex.Source + ":" + ex.Message;
                }
                catch
                {
                    throw exception;
                }
            }
        }
    }
}
