using System;
using System.Collections.Generic;
using System.Text;
using Chempoint.GP.Model.Interactions.Sales;
using ChemPoint.GP.BusinessContracts.Sales;
using System.Xml;
using ChemPoint.GP.Entities.Business_Entities;
using ChemPoint.GP.Entities.BaseEntities;
using ChemPoint.TaxEngine;
using ChemPoint.GP.DataContracts.Sales;
using System.Xml.Xsl;
using System.IO;
using Microsoft.Dynamics.GP.eConnect;
using ChemPoint.GP.XrmServices;
using Chempoint.GP.Model.Interactions.AuditLog;
using ChemPoint.GP.CPServiceAuditLogBL;
using ChemPoint.GP.SalesOrderBL;
using Chempoint.GP.Infrastructure.Logging;
using Chempoint.GP.Infrastructure.Email;
using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.BusinessContracts.TaskScheduler.HoldEngine;
using ChemPoint.GP.HoldEngineBL;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;
using System.Data;

namespace ChemPoint.GP.SalesOrderCreateBL
{
    public class SalesOrderCreateBusiness : ISalesOrderCreateBI
    {
        public SalesOrderCreateBusiness()
        {
        }

        public SalesOrderResponse CreateSalesOrder(SalesOrderRequest salesOrderRequest)
        {

            StringBuilder logMessage = new StringBuilder();

            SalesOrderResponse response = new SalesOrderResponse();
            response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;

            salesOrderRequest.eComXML = salesOrderRequest.eComXML.Replace(":ns0", string.Empty).Replace("ns0:", string.Empty);
            salesOrderRequest.eComXML = salesOrderRequest.eComXML.Replace("xmlns=\"urn:schemas-biztalk-org:Chempoint\"", string.Empty);

            // Clear the Last Error
            string conString = string.Empty;
            string exception = string.Empty;
            bool blResult = false;
            string xrmResult;
            DateTime dtShipDate = DateTime.MaxValue;
            int iShipdateDuration = 0;

            eConnectMethods eConObj = null;
            StringBuilder auditLog = new StringBuilder();

            try
            {
                logMessage.AppendLine("****************************************************************.");
                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - Strated pushing the ecom order to GP");

                logMessage.AppendLine(DateTime.Now.ToString() + " - Ecom Order xml: " + salesOrderRequest.eComXML);

                //Update Service SKU line Item
                // loads the data from XML and maps to object
                MapXmlOrderToEntity(ref salesOrderRequest, ref logMessage);

                //Call DB to get the sales order details
                logMessage.AppendLine(DateTime.Now.ToString() + " - Calling DB to get the details of the sales order.");
                ISalesOrderUpdateRepository salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
                SalesOrderResponse salesOrderDetailsResponse = salesDataAccess.GetOrderDetailsForPushOrderToGP(salesOrderRequest, salesOrderRequest.CompanyID);
                salesOrderRequest.SalesOrderEntity = salesOrderDetailsResponse.SalesOrderDetails;

                // Update ecom XML with tax and uom details
                logMessage.AppendLine(DateTime.Now.ToString() + " - Updating XML with OM details.");
                salesOrderRequest.eComXML = AddUomDetailsToXml(salesOrderRequest);

                #region TAX_Details

                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - -- Calculating Tax details");
                TaxResult taxResult = new TaxResult();
                // Get the tax details
                if (salesOrderRequest.SalesOrderEntity.CompanyName.Equals("Chmpt"))
                    taxResult = CalculateTax(salesOrderRequest, ref logMessage);

                // Update Tax details to Ecom xml
                salesOrderRequest.eComXML = AddTaxDetails(salesOrderRequest, taxResult, ref logMessage);

                #endregion TAX_Details

                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - Ecom XML with tax: " + salesOrderRequest.eComXML);

                // Transform from ChemMsgEcomOrder to eConnect
                salesOrderRequest.eConnectXML = TransformToEconnectXml(salesOrderRequest.eComXML,
                    salesOrderRequest.StyleSheetPath,
                    salesOrderRequest.SalesOrderEntity.CompanyName,
                    salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightDecimalPlaces,
                    salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ExtendedDecimalPlaces);

                logMessage.AppendLine(DateTime.Now.ToString() + " - Econnect XML :" + salesOrderRequest.eConnectXML);
                // Connection string for eConnect
                if (salesOrderRequest.SalesOrderEntity.CompanyName.Equals("Chmpt"))
                    conString = salesOrderRequest.NAEconnectConnectionString;
                else
                    conString = salesOrderRequest.EUEconnectConnectionString;

                logMessage.AppendLine(DateTime.Now.ToString() + " - Connection String : " + conString);

                // Object for eConnect
                eConObj = new eConnectMethods();
                logMessage.AppendLine(DateTime.Now.ToString() + " - Created eConnect Method");
                // Call the eConnect method to update the tables.
                blResult = eConObj.eConnect_EntryPoint(conString, Microsoft.Dynamics.GP.eConnect.EnumTypes.ConnectionStringType.SqlClient, salesOrderRequest.eConnectXML, Microsoft.Dynamics.GP.eConnect.EnumTypes.SchemaValidationType.XSD, "");
                logMessage.AppendLine(DateTime.Now.ToString() + " - eConnect_EntryPoint Creation is " + blResult);
                //checks for transaction success or failureLogMessage(
                if (blResult == true)
                {
                    logMessage.AppendLine(DateTime.Now.ToString() + " - Order successfully pushed.");
                    response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                    //Log to the datatable 
                    auditLog.Append("Order Successfully pushed in to GP.");

                    xrmResult = new XrmService().PublishSalesOrderStatus(salesOrderRequest.SalesOrderEntity.SopNumber, salesOrderRequest.SalesOrderEntity.CompanyName, salesOrderRequest.XrmServiceUrl, "CpEconnectUser", "OrderPushToGP");
                    logMessage.AppendLine(DateTime.Now.ToString() + " - XRM result - " + xrmResult);

                    logMessage.AppendLine(DateTime.Now.ToString() + " - Before PTE Log ");
                    //Call PTE
                    if (salesOrderRequest.CompanyID == 1)
                    {
                        new SalesOrderBusiness().UpdatePteLog(salesOrderRequest);
                    }
                    //Call Hold Engine
                    HoldEngineRequest holdEngineRequest = new HoldEngineRequest();
                    HoldEngineResponse holdEngineResponse = new HoldEngineResponse();
                    HoldEngineEntity holdEngineEntity = new HoldEngineEntity();
                    CustomerInformation customerInformation = new CustomerInformation();
                    OrderHeader orderHeader = new OrderHeader();
                    orderHeader.SopNumber = salesOrderRequest.SalesOrderEntity.SopNumber.ToString().Trim();
                    orderHeader.SopType = salesOrderRequest.SalesOrderEntity.SopType;
                    customerInformation.CustomerId = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId.ToString().Trim();
                    holdEngineEntity.BatchUserID = "batch";
                    holdEngineEntity.CustomerInformation = customerInformation;
                    holdEngineEntity.OrderHeader = orderHeader;
                    holdEngineRequest.HoldEngineEntity = holdEngineEntity;
                    holdEngineRequest.ConnectionString = salesOrderRequest.ConnectionString;
                    holdEngineRequest.CompanyId = Convert.ToInt16(salesOrderRequest.CompanyID);

                    IHoldEngineBusiness creditHold = new HoldEngineBusiness();
                    holdEngineResponse = creditHold.ProcessHoldForOrder(holdEngineRequest);

                    if (holdEngineResponse.Status == Chempoint.GP.Model.Interactions.HoldEngine.HoldEngineResponseStatus.Success)
                    {
                        response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Success;
                    }

                    //Call Commit engine
                    new SalesOrderBusiness().RunCommitmentEngine(salesOrderRequest);
                }
                else
                {
                    // Return failure to the caller.
                    auditLog.Append("Error: Order has failed to submit in GP.");
                    response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                    response.ErrorMessage = "Error: Order has failed to submit in GP.";
                }
            }
            catch (eConnectException eConex)
            {
                response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                response.ErrorMessage = eConex.Message.ToString();
                logMessage.AppendLine(DateTime.Now.ToString() + "Econnect Error Occured");
                // Check whether the Exception has value.
                if (string.IsNullOrEmpty(eConex.Message.ToString()))
                {
                    // Get the un-anticipated error from the ChempointConfiguration xml.
                    exception = "Unknown error occurred.";
                }
                else
                {
                    exception = eConex.Message;
                }
                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + exception);
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + eConex.StackTrace);

                if ((dtShipDate - DateTime.Today).TotalDays <= iShipdateDuration)
                {
                    salesOrderRequest.SalesPriorityOrdersEmail.Subject = salesOrderRequest.SalesPriorityOrdersEmail.Subject + salesOrderRequest.SalesOrderEntity.SopNumber;
                    salesOrderRequest.SalesPriorityOrdersEmail.Body = BuildMailBody(exception, salesOrderRequest.eComXML, salesOrderRequest.eConnectXML, salesOrderRequest.SalesOrderEntity.SopNumber, "SOP", salesOrderRequest.MailStyleSheetPath);

                    if (new EmailHelper().SendMail(salesOrderRequest.SalesPriorityOrdersEmail))
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Order failure but date within 5 days email has been successfully sent");
                    else
                        logMessage.AppendLine(DateTime.Now.ToString() + " - Order failure but date within 5 days email failed to send.");
                }
            }
            catch (Exception ex)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:InnerException " + (ex.InnerException != null ? ex.InnerException.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:Source " + (ex.Source != null ? ex.Source.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:Data " + (ex.Data != null ? ex.Data.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:Target Site " + (ex.TargetSite != null ? ex.TargetSite.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:Stack Trace " + (ex.StackTrace != null ? ex.StackTrace.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:Message " + (ex.Message != null ? ex.Message.ToString() : string.Empty));
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error:HResult " + (ex.HResult != null ? ex.HResult.ToString() : string.Empty));

                response.Status = Chempoint.GP.Model.Interactions.Sales.ResponseStatus.Error;
                response.ErrorMessage = ex.Message.ToString() + " StackStrace: " + ex.StackTrace.ToString();
                // Check whether the Exception has value.
                if (string.IsNullOrEmpty(ex.Message.ToString()))
                {
                    // Get the un-anticipated error from the ChempointConfiguration xml.
                    exception = "Unknown error occurred.";
                }
                else
                {
                    exception = ex.ToString();
                }

                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + exception);
                logMessage.AppendLine(DateTime.Now.ToString() + " - Error: " + ex.StackTrace);

                if ((dtShipDate - DateTime.Today).TotalDays < iShipdateDuration)
                {
                    salesOrderRequest.SalesPriorityOrdersEmail.Subject = string.Format("SOP is Failed (Biztalk) for the SOPNumber: {0} on first iteration", salesOrderRequest.SalesOrderEntity.SopNumber);
                    salesOrderRequest.SalesPriorityOrdersEmail.Body = string.Format("This Order {0} failed while pushing to gp on first iteration, due to the following error detail: {1}.", salesOrderRequest.SalesOrderEntity.SopNumber, exception);
                }
            }
            finally
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - Updating XRM log table.");
                if (!string.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.SopNumber))
                {
                    UpdateXrmIntegrationsAuditLog(salesOrderRequest.Source, "Status Update", salesOrderRequest.SalesOrderEntity.SopNumber, auditLog.ToString(), salesOrderRequest.SalesOrderEntity.CompanyName);
                }
                logMessage.AppendLine(DateTime.Now.ToString() + " - SalesOrderMessageProcess ended.");
                logMessage.AppendLine("****************************************************************.");
                //log the message
                new TextLogger().LogInformationIntoFile(logMessage.ToString(), salesOrderRequest.LoggingPath, salesOrderRequest.LoggingFileName);
            }
            return response;
        }


        /// <summary>
        /// Update Service SKU Line item to Ecom XML
        /// </summary>
        /// <param name="salesOrderRequest"></param>
        /// <param name="logMessage"></param>
        /// <param name="serviceSku"></param>
        private string UpdateServiceSKULineItem(ref SalesOrderRequest salesOrderRequest, ref StringBuilder logMessage, SalesOrderResponse serviceSku)
        {
            logMessage.AppendLine(DateTime.Now.ToString() + " - AddServiceSKU method started ");//totest
            int lineNumber = 1;
            string warehouseId = string.Empty;

            string[] excludeMiscForServiceSKU = Chempoint.GP.Infrastructure.Config.Configuration.ExcludeForServiceSKU.Split(',');
            // Create object for XmlDocument and load the xml.
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(salesOrderRequest.eComXML);

            XmlNodeList LineNumberList = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");
            warehouseId = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine", "ShipFromWarehouseId", false);

            foreach (XmlNode line in LineNumberList)
            {
                lineNumber = Convert.ToInt32(line.Attributes["LineNo"].Value);
            }

            List<SalesLineItem> salesOrderSKU = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems;
            foreach (SalesLineItem lineItem in serviceSku.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItems)
            {
                #region ValueDeclaration
                //  set ExpeditedCharges as default for particular sku.
                foreach (string miscAmountExclude in excludeMiscForServiceSKU)
                {
                    if (lineItem.ItemNumber.ToString() == miscAmountExclude)
                    {
                        XmlNode DefaultMiscAmount = xmlDoc.SelectSingleNode("/CreateEcomOrder/Orders/Order");
                        DefaultMiscAmount.Attributes["ExpeditedCharges"].Value = "0.0000";
                    }
                }

                XmlNode OrderLines = xmlDoc.SelectSingleNode("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");

                XmlNode SelectOrderLine = xmlDoc.SelectSingleNode("/CreateEcomOrder/Orders/Order/OrderLines");

                XmlNode OrderLine = xmlDoc.CreateNode(XmlNodeType.Element, "OrderLine", null);
                XmlNode Item = xmlDoc.CreateNode(XmlNodeType.Element, "Item", null);
                XmlNode LinePriceInfo = xmlDoc.CreateNode(XmlNodeType.Element, "LinePriceInfo", null);
                XmlNode FreightInfo = xmlDoc.CreateNode(XmlNodeType.Element, "FreightInfo", null);

                XmlAttribute lineGuid = xmlDoc.CreateAttribute("LineGuid");
                XmlAttribute lineNo = xmlDoc.CreateAttribute("LineNo");
                XmlAttribute orderedQty = xmlDoc.CreateAttribute("OrderedQty");
                XmlAttribute shipFromWarehouseId = xmlDoc.CreateAttribute("ShipFromWarehouseId");
                XmlAttribute quoteGuid = xmlDoc.CreateAttribute("QuoteGuid");
                XmlAttribute quoteNumber = xmlDoc.CreateAttribute("QuoteNumber");
                XmlAttribute quoteName = xmlDoc.CreateAttribute("QuoteName");
                lineNumber++;
                lineGuid.Value = "";
                lineNo.Value = lineNumber.ToString();
                orderedQty.Value = "1";
                shipFromWarehouseId.Value = warehouseId;
                quoteGuid.Value = "";
                quoteNumber.Value = "-";
                quoteName.Value = "";

                XmlAttribute itemID = xmlDoc.CreateAttribute("ItemID");
                XmlAttribute uofm = xmlDoc.CreateAttribute("UnitOfMeasure");
                XmlAttribute itemDesc = xmlDoc.CreateAttribute("ItemDesc");

                XmlAttribute frightAmount = xmlDoc.CreateAttribute("FreightAmount");
                XmlAttribute pricePerunit = xmlDoc.CreateAttribute("PricePerUnit");
                XmlAttribute pricePerSku = xmlDoc.CreateAttribute("PricePerSku");
                XmlAttribute lineTotal = xmlDoc.CreateAttribute("LineTotal");
                XmlAttribute discountAmount = xmlDoc.CreateAttribute("DiscountAmount");

                XmlAttribute freightTerms = xmlDoc.CreateAttribute("FreightTerms");
                XmlAttribute freightService = xmlDoc.CreateAttribute("FreightService");
                XmlAttribute incoTerms = xmlDoc.CreateAttribute("IncoTerms");
                XmlAttribute shipVia = xmlDoc.CreateAttribute("ShipVia");
                XmlAttribute carrierDescription = xmlDoc.CreateAttribute("CarrierDescription");
                XmlAttribute carrierRegion = xmlDoc.CreateAttribute("CarrierRegion");
                XmlAttribute carrierAccount = xmlDoc.CreateAttribute("CarrierAccount");
                XmlAttribute carrierPhoneNo = xmlDoc.CreateAttribute("CarrierPhoneNo");
                XmlAttribute isFIP = xmlDoc.CreateAttribute("IsFIP");
                XmlAttribute freightPerUnit = xmlDoc.CreateAttribute("FreightPerUnit");

                #endregion ValueDeclaration

                #region AssignValuetoEcomXML


                itemID.Value = lineItem.ItemNumber;
                uofm.Value = lineItem.ItemUofM;
                itemDesc.Value = lineItem.ItemDescription;

                frightAmount.Value = "0";
                pricePerunit.Value = lineItem.UnitPriceAmount.ToString();
                pricePerSku.Value = lineItem.UnitCostAmount.ToString();
                lineTotal.Value = "0";
                discountAmount.Value = "0";

                freightTerms.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.FreightTerm; ;
                freightService.Value = salesOrderSKU[0].FreightService;
                incoTerms.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.IncoTerm;
                shipVia.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipVia;
                carrierDescription.Value = salesOrderSKU[0].CarrierDescription;
                if (salesOrderRequest.CompanyID == 1)
                    carrierRegion.Value = "NA";
                else
                    carrierRegion.Value = "EU";
                carrierAccount.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CarrierAccountNumber;
                carrierPhoneNo.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.CarrierPhone;
                isFIP.Value = "false";
                freightPerUnit.Value = "0";
                #endregion AssignValuetoEcomXML

                #region AddTOEcomXML
                // adds the ServiceSKU attributes  to the line items

                Item.Attributes.Append(itemID);
                Item.Attributes.Append(uofm);
                Item.Attributes.Append(itemDesc);

                LinePriceInfo.Attributes.Append(frightAmount);
                LinePriceInfo.Attributes.Append(pricePerunit);
                LinePriceInfo.Attributes.Append(pricePerSku);
                LinePriceInfo.Attributes.Append(lineTotal);
                LinePriceInfo.Attributes.Append(discountAmount);

                FreightInfo.Attributes.Append(freightTerms);
                FreightInfo.Attributes.Append(freightService);
                FreightInfo.Attributes.Append(incoTerms);
                FreightInfo.Attributes.Append(shipVia);
                FreightInfo.Attributes.Append(carrierDescription);
                FreightInfo.Attributes.Append(carrierRegion);
                FreightInfo.Attributes.Append(carrierAccount);
                FreightInfo.Attributes.Append(carrierPhoneNo);
                FreightInfo.Attributes.Append(isFIP);
                FreightInfo.Attributes.Append(freightPerUnit);

                OrderLine.AppendChild(Item);
                OrderLine.AppendChild(LinePriceInfo);
                OrderLine.AppendChild(FreightInfo);

                OrderLine.Attributes.Append(lineGuid);
                OrderLine.Attributes.Append(lineNo);
                OrderLine.Attributes.Append(orderedQty);
                OrderLine.Attributes.Append(shipFromWarehouseId);
                OrderLine.Attributes.Append(quoteGuid);
                OrderLine.Attributes.Append(quoteNumber);
                OrderLine.Attributes.Append(quoteName);

                SelectOrderLine.AppendChild(OrderLine);

                XmlNode NumberOfOrderLines = xmlDoc.SelectSingleNode("/CreateEcomOrder/Orders/Order/OrderLines/NumberOfOrderLines");
                NumberOfOrderLines.InnerText = lineNumber.ToString();
                #endregion AddTOEcomXML
            }

            return xmlDoc.InnerXml.Trim();

        }



        /// <summary>
        /// Update audit log for XRM
        /// </summary>
        /// <param name="source">Specify source name</param>
        /// <param name="operation">Specify operation name</param>
        /// <param name="sopNumber">Specify order number</param>
        /// <param name="notes">Specify notes</param>
        private bool UpdateXrmIntegrationsAuditLog(string source, string operation, string sopNumber, string notes, string companyId)
        {
            bool result = false;
            try
            {
                XrmAuditLogRequest request = new XrmAuditLogRequest();
                request.AuditLogEntity.Source = source;
                request.AuditLogEntity.Operation = operation;
                request.AuditLogEntity.SourceDocumentId = sopNumber;
                request.AuditLogEntity.Notes = notes;
                request.AuditLogEntity.Company = (companyId == "Chmpt" ? 1 : 2);
                request.AuditLogEntity.UserID = "CpEconnectUser";
                request.AuditLogEntity.CreatedOn = DateTime.Now;
                var response = new XrmAuditLogBusiness().UpdateXrmIntegrationsAuditLog(request);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error: " + ex.Message, Resources.STR_MESSAGE_TITLE);
            }
            finally
            {
            }
            return result;
        }

        /// <summary>
        /// Transform eComOrderXml to eConnect XML
        /// </summary>
        /// <param name="eComOrderXml">eComOrderXML as string</param>
        /// <returns>eConnectXml as string</returns>
        private string TransformToEconnectXml(string eComOrderXml, string styleSheetPath, string companyId, int freightDecimalPlaces, int extendedDecimalPlaces)
        {
            //Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();

            //Create object for XslTransform 
            XslCompiledTransform xslTrans = new XslCompiledTransform();

            //Creating Argument List Object
            XsltArgumentList xsltArgs = new XsltArgumentList();

            //Creating StringWriter Object
            StringWriter sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);

            // Local variables.
            string transformedXml = string.Empty;

            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(eComOrderXml);

            //Loading XSL
            xslTrans.Load(styleSheetPath);

            // Add XSLT parameters.
            AddXsltParameters(xsltArgs, companyId.ToUpper(), freightDecimalPlaces, extendedDecimalPlaces);

            //Peroforming transformation
            xslTrans.Transform(xmlDoc, xsltArgs, sWriter);

            // Set the transformed xml.
            transformedXml = sWriter.ToString().Trim();

            // Dispose the objects.                                                
            sWriter.Dispose();

            // Return the transformed xml to the caller.
            return transformedXml;
        }

        /// <summary>
        /// Builds the mail body using xsl.
        /// </summary>
        /// <param name="exceptionDetails">exceptionDetails as string</param>
        /// <param name="eComOrderXml">eComOrderXml as string</param>
        /// <param name="eConnectXml">eConnectXml as string</param>
        /// <param name="orderNumber">orderNumber as string</param>
        /// <param name="source">source as string</param>
        /// <returns>TransformedXml as string</returns>
        private static string BuildMailBody(string exceptionDetails, string eComOrderXml, string eConnectXml, string orderNumber, string source, string mailStyleSheet)
        {
            // ****************************** Mask credit card info: -
            string eComOrderXmlNoCC = string.Empty;

            eComOrderXmlNoCC = eComOrderXml.Replace("xmlns=\"" + "urn:schemas-biztalk-org:Chempoint" + "\"", "");
            XmlDocument oXmlDoc = new XmlDocument();
            oXmlDoc.LoadXml(eComOrderXmlNoCC);

            eComOrderXmlNoCC = oXmlDoc.InnerXml;
            // ******************************

            // Creating XmlDocument Object.
            XmlDocument xmlDoc = new XmlDocument();

            // Creating transform Object.
            XslCompiledTransform xslTrans = new XslCompiledTransform();

            // Creating Argument List Object.
            XsltArgumentList xsltArgs = new XsltArgumentList();

            //Creating StringWriter Object.
            StringWriter sWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);

            string transformedXml = string.Empty;

            // Loading XSL.
            xslTrans.Load(mailStyleSheet);

            // Adding Parameter to xslt.
            xsltArgs.AddParam("ExceptionDetails", "", (object)exceptionDetails);
            xsltArgs.AddParam("EcomOrderXml", "", (object)eComOrderXmlNoCC);
            xsltArgs.AddParam("eConnectXml", "", (object)eConnectXml);
            xsltArgs.AddParam("OrderNumber", "", (object)orderNumber);
            xsltArgs.AddParam("Source", "", (object)source);

            //Peroforming transformation.
            xslTrans.Transform(xmlDoc, xsltArgs, sWriter);

            // Set the tranformed xml.
            transformedXml = sWriter.ToString().Trim();

            // Dispose the object.
            sWriter.Dispose();

            // Return the transformed xml to the caller.
            return transformedXml;
        }

        /// <summary>
        /// Add XSLT parameters
        /// </summary>
        /// <param name="xsltArgs">xsltArgs as XsltArgumentList</param>
        /// <param name="companyId">companyId as string</param>
        /// <param name="extendedDecimal">extendedDecimal as int</param>
        /// <param name="freightDecimal">freightDecimal as int</param>
        private void AddXsltParameters(XsltArgumentList xsltArgs, string companyId, int freightDecimalPlaces, int extendedDecimalPlaces)
        {
            // Adding Parameter to xslt(USER2ENT field in eConnect)
            string strUserID = System.Environment.UserName;
            xsltArgs.AddParam("UserID", "", (object)strUserID);

            // Adding the today parameter (BACHNUMB field in eConnect)
            string currentDate = System.DateTime.Today.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture).Replace("-", "");
            xsltArgs.AddParam("BatchNumber", "", (object)currentDate);

            // Adding the CompanyID parameter
            xsltArgs.AddParam("CompanyID", "", (object)companyId);

            // Adding the parameters for the decimal places            
            xsltArgs.AddParam("ExtendedDecimal", "", (object)extendedDecimalPlaces);
            xsltArgs.AddParam("FreightDecimal", "", (object)freightDecimalPlaces);
        }

        private string AddUomDetailsToXml(SalesOrderRequest salesOrderRequest)
        {
            // Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();
            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(salesOrderRequest.eComXML);

            // Update the UOM details
            XmlNodeList orderLines = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");
            foreach (XmlNode order in orderLines)
            {
                foreach (SalesLineItem line in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
                {
                    if (line.ItemNumber.Equals(order["Item"].Attributes["ItemID"].Value))
                    {
                        order["Item"].Attributes["UnitOfMeasure"].Value = line.ItemUofM.Trim();
                        order["Item"].SetAttribute("DecimalPlaces", "", (line.CurrencyDecimalPlaces).ToString(System.Globalization.CultureInfo.CurrentCulture));
                        break;
                    }
                }
            }
            return xmlDoc.InnerXml.Trim();
        }

        private TaxResult CalculateTax(SalesOrderRequest salesOrderRequest, ref StringBuilder logMessage)
        {

            // tax result object 
            TaxResult result;

            #region Building_TaxRequestObject

            // tax request object 
            TaxRequest taxRequest = new TaxRequest();
            taxRequest.DocumentDate = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderDate;
            // company code
            taxRequest.CmpyCode = CompanyCodeEnum.Chmpt;
            taxRequest.DocumentNumber = salesOrderRequest.SalesOrderEntity.SopNumber;
            // Customer ID                    
            taxRequest.CustomerNumber = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerId;
            // Currency id
            taxRequest.OrderCurrencyCode = (salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.OrderCurrency == "Z-US$" ? CurrencyCodeEnum.USD : CurrencyCodeEnum.CAD);
            // customer exemption number
            taxRequest.CustomerExemptionNo = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxExemptCode;


            taxRequest.CustomerUseCode = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxUseCode;

            #endregion

            #region LineDetails_TaxRequest

            //Line details
            foreach (SalesLineItem saleLine in salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems)
            {
                // for every line creates a LineDetail Object
                LineDetail line = new LineDetail();
                line.ItemNumber = saleLine.ItemNumber;
                line.LineSeq = saleLine.OrderLineId;
                line.LineExtendedPrice = saleLine.LineTotalAmount;
                line.LineExemptionNo = saleLine.OrderLineExemptionNo;
                line.LineQty = Convert.ToDouble(saleLine.OrderedQuantity);
                line.LineShipMethod = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipVia;
                line.LineShippingType = (salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipViaType.ToUpper() == "PICKUP" ? LineShippingType.Pickup : LineShippingType.Delivery);

                // sets Ship to address
                line.ShipToAddressLine1 = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.AddressLine1;
                line.ShipToAddressLine2 = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.AddressLine2;
                line.ShipToAddressCity = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.City;
                line.ShipToAddressState = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.State;
                line.ShipToAddressZip = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.ZipCode;
                line.ShipToAddressCountry = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.OrderHeader.ShipToAddress.Country;

                // sets Ship from address
                line.ShipFromAddressLine1 = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Address1;
                line.ShipFromAddressLine2 = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Address2;
                line.ShipFromAddressCity = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.City;
                line.ShipFromAddressState = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.State;
                line.ShipFromAddressZip = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.ZipCode;
                line.ShipFromAddressCountry = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Warehouse.Country;

                // adds the LineDetail to TaxRequest.
                taxRequest.AddLineDetail(line);
            }

            #endregion LineDetails_TaxRequest

            logMessage.AppendLine(DateTime.Now.ToString() + " - Building_TaxRequestObject ended ");//totest
            try
            {
                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - -- Calling the Web service for tax calculation.");

                // calculates tax
                TaxCalculator calc = new TaxCalculator();
                result = calc.CalculateTax(taxRequest);

                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - -- Successfully completed calculating tax.");
            }
            catch (Exception ex)
            {
                // Log message.
                logMessage.AppendLine(DateTime.Now.ToString() + " - -- Error while calculating tax. :" + ex.StackTrace);

                logMessage.AppendLine(DateTime.Now.ToString() + " - -- Hence pushing the order with tax as 0.");

                result = new TaxResult();
            }
            return result;
        }

        private string AddTaxDetails(SalesOrderRequest salesOrderRequest, TaxResult result, ref StringBuilder logMessage)
        {
            logMessage.AppendLine(DateTime.Now.ToString() + " - AddTaxDetails method started ");//totest

            // Create object for XmlDocument and load the xml.
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(salesOrderRequest.eComXML);

            #region AddTaxNodes_LineEcomXML

            // Adds the tax nodes at Line level
            XmlNodeList orderLines = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");
            foreach (XmlNode order in orderLines)
            {
                XmlAttribute lineTaxAmt = xmlDoc.CreateAttribute("LineTaxAmt");
                XmlAttribute lineTaxRate = xmlDoc.CreateAttribute("LineTaxRate");
                lineTaxAmt.Value = "0";
                lineTaxRate.Value = "0";
                if (result.TaxLineDetails != null)
                {
                    foreach (TaxLineDetail taxDetail in result.TaxLineDetails)
                    {
                        if (taxDetail.LineSeq.ToString().Equals(order.Attributes["LineNo"].Value))
                        {
                            lineTaxAmt.Value = taxDetail.Tax.ToString();
                            if (taxDetail.Tax > 0)
                                lineTaxRate.Value = taxDetail.Rate.ToString();
                        }
                    }
                }
                // adds the tax attributes  to the line items
                order["LinePriceInfo"].Attributes.Append(lineTaxAmt);
                order["LinePriceInfo"].Attributes.Append(lineTaxRate);
            }

            #endregion AddTaxNodes_LineEcomXML

            #region AddTaxNodes_HeaderEcomXML

            // Adds the tax nodes at header level
            XmlAttribute orderTaxRate = xmlDoc.CreateAttribute("OrderTaxRate");
            orderTaxRate.Value = "0";
            if (result.TaxLineDetails != null)
            {
                foreach (TaxLineDetail taxDetail in result.TaxLineDetails)
                {
                    if (taxDetail.LineSeq.ToString().Equals("1") && taxDetail.Tax > 0)
                    {
                        orderTaxRate.Value = taxDetail.Rate.ToString();
                    }
                }
            }
            XmlNodeList orders = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order");
            XmlNode orderNode = orders[0];
            orderNode.Attributes.Append(orderTaxRate);

            // Append the tax exempt value.
            XmlAttribute orderTaxExempt = xmlDoc.CreateAttribute("OrderTaxExempt");
            orderTaxExempt.Value = "";
            if (!string.IsNullOrEmpty(salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxExemptCode))
            {
                orderTaxExempt.Value = salesOrderRequest.SalesOrderEntity.SalesOrderDetails.Customer.CustomerTaxExemptCode;
            }
            orderNode.Attributes.Append(orderTaxExempt);

            #endregion AddTaxNodes_HeaderEcomXML

            return xmlDoc.InnerXml;
        }

        private void MapXmlOrderToEntity(ref SalesOrderRequest salesOrderRequest, ref StringBuilder logMessage)
        {
            // Create object for XmlDocument
            XmlDocument xmlDoc = new XmlDocument();

            SalesOrderEntity salesOrder = new SalesOrderEntity();
            CustomerInformation customer = new CustomerInformation();
            OrderHeader oheader = new OrderHeader();
            WarehouseInformation warehouse = new WarehouseInformation();
            SalesOrderInformation orderInfo = new SalesOrderInformation();
            SalesOrderDetails order = new SalesOrderDetails();
            List<SalesLineItem> lines = new List<SalesLineItem>();
            AddressInformation shippingAddress = new AddressInformation();
            AuditInformation audit = new AuditInformation();
            OrderSchedule orderSchedule = new OrderSchedule();
            SalesOrderResponse serviceSku = new SalesOrderResponse();
            SalesOrderRequest serviceSKURequest = new SalesOrderRequest();
            SalesOrderType orderType = new SalesOrderType();
            //Loading ChemMsgEcomOrder XML
            xmlDoc.LoadXml(salesOrderRequest.eComXML);

            // Get the OrderNumber from the ChemMsgEcomOrder xml.
            salesOrder.SopNumber = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "OrderNo", false);
            logMessage.AppendLine(DateTime.Now.ToString() + " - --Order Number: " + salesOrder.SopNumber);

            // Sop Type set
            salesOrder.SopType = 2;
            // Get the CompanyId from the ChemMsgEcomOrder xml.
            salesOrder.CompanyName = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "CPCompanyId", false) == "North America" ? "Chmpt" : "Cpeur");
            salesOrderRequest.CompanyID = (salesOrder.CompanyName.ToLower() == "chmpt" ? 1 : 2);
            logMessage.AppendLine(DateTime.Now.ToString() + " - --Commpany Name: " + salesOrder.CompanyName);

            // Get the CustomerId node and its value.
            customer.CustomerId = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoMain", "CustomerID", false);
            logMessage.AppendLine(DateTime.Now.ToString() + " - --Customer ID: " + customer.CustomerId);

            // Get the CurrencyId node and its value.
            oheader.OrderCurrency = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "CurrencyID", false);
            // Verify the currency id for USD and change to Z-US$
            if (oheader.OrderCurrency.Equals("USD"))
                oheader.OrderCurrency = "Z-US$";
            logMessage.AppendLine(DateTime.Now.ToString() + " - -- Order Currency: " + oheader.OrderCurrency);

            oheader.MiscAmount = Convert.ToDecimal(GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "ExpeditedCharges", false));

            orderType.IsAutoPTEligible = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderStatus", "IsAutoPTEligible", false) == "true" ? 1 : 0);
            orderType.IsCreditEnginePassed = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderStatus", "IsCreditEnginePassed", false) == "true" ? 1 : 0);
            orderType.IsTaxEnginePassed = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderStatus", "IsTaxEnginePassed", false) == "true" ? 1 : 0);

            orderType.IsInternational = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsInternational", false) == "true" ? 1 : 0);
            orderType.IsNaFta = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsNAFTA", false) == "true" ? 1 : 0);
            orderType.IsDropship = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsDropship", false) == "true" ? 1 : 0);
            orderType.IsBulk = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsBulk", false) == "true" ? 1 : 0);
            orderType.IsI3Order = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsI3ORder", false) == "true" ? 1 : 0);
            orderType.IsCreditCard = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsCreditCard", false) == "true" ? 1 : 0);
            orderType.IsCorrective = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsCorrective", false) == "true" ? 1 : 0);
            orderType.IsFullTruckLoad = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsFullTruckLoad", false) == "true" ? 1 : 0);
            orderType.IsConsignment = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsConsignment", false) == "true" ? 1 : 0);
            orderType.IsCorrectiveBoo = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsCorrectiveBOO", false) == "true" ? 1 : 0);
            orderType.IsHazmat = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsHazmat", false) == "true" ? 1 : 0);
            orderType.IsTempControl = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsTempControl", false) == "true" ? 1 : 0);
            orderType.IsFreezeProtect = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsFreezeProtect", false) == "true" ? 1 : 0);
            orderType.IsSampleOrder = (GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderType", "IsSample", false) == "true" ? 1 : 0);

            // Service SKU Validation Mandatory field...
            XmlNodeList serviceSKUorderLines = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");
            foreach (XmlNode serviceSKUline in serviceSKUorderLines)
            {
                SalesLineItem lineItem = new SalesLineItem();
                lineItem.ItemNumber = serviceSKUline["Item"].Attributes["ItemID"].Value.Trim();
                lineItem.OrderedQuantity = Convert.ToDecimal(serviceSKUline.Attributes["OrderedQty"].Value);
                lineItem.CarrierDescription = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "CarrierDescription", false);
                lineItem.FreightService = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "FreightService", false);
                lines.Add(lineItem);
            }

            warehouse.WarehouseId = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine", "ShipFromWarehouseId", false);

            oheader.FreightTerm = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "FreightTerms", false);

            oheader.CarrierAccountNumber = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "CarrierAccount", false);

            oheader.CarrierPhone = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "CarrierPhoneNo", false);

            oheader.ShipVia = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "ShipVia", false);

            oheader.IncoTerm = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "IncoTerms", false);

            // Get the ShipVia and its value.
            oheader.ShipVia = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/OrderLines/OrderLine/FreightInfo", "ShipVia", false);

            string orderDateLocal = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "OrderDate", false);
            oheader.OrderDate = DateTime.ParseExact(orderDateLocal, "yyyyMMddHHmmss", null);
            orderSchedule.OrderCreatedDate = DateTime.ParseExact(orderDateLocal, "yyyyMMddHHmmss", null);

            string reqShipDateLocal = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "CurrentScheduledShipDate", false);
            orderSchedule.RequestedShipDate = DateTime.ParseExact(reqShipDateLocal, "yyyyMMddHHmmss", null);

            string orderSubmittedDate = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/AuditInfo", "ModifiedDate", false);
            oheader.OrderSubmittedDate = DateTime.ParseExact(orderSubmittedDate, "yyyyMMddHHmmss", null);

            //Shipping address
            shippingAddress.AddressLine1 = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "Address1", false);
            shippingAddress.AddressLine2 = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "Address2", false);
            shippingAddress.City = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "City", false);
            shippingAddress.State = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "State", false);
            shippingAddress.ZipCode = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "ZipCode", false);
            shippingAddress.Country = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order/PersonInfoFinalDestinationTo", "Country", false);

            //Get User ID
            salesOrderRequest.UserID = GetXmlAttributeValue(xmlDoc, "/CreateEcomOrder/Orders/Order", "AccountOwnerId", false);
            logMessage.AppendLine(DateTime.Now.ToString() + " - --Order Number: " + salesOrderRequest.UserID);
            salesOrderRequest.Source = "NEWORDER";

            audit.ModifiedBy = "orderPush";  // default value
            audit.CompanyId = 1; // default value

            //Object assigning...
            oheader.ShipToAddress = shippingAddress;
            order.OrderHeader = oheader;
            order.LineItems = lines;
            order.SalesOrderType = orderType;
            order.OrderSchedule = orderSchedule;
            order.AuditInformation = audit;
            orderInfo.SalesOrderDetails = order;
            orderInfo.Warehouse = warehouse;
            orderInfo.Customer = customer;
            salesOrder.SalesOrderDetails = orderInfo;
            salesOrderRequest.SalesOrderEntity = salesOrder;

            // Service SKU validation Call Start...
            ISalesOrderUpdateRepository salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(salesOrderRequest.ConnectionString);
            serviceSku = salesDataAccess.GetServiceSKULineItem(salesOrderRequest);

            if (serviceSku.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItems.Count != 0 && serviceSku.SalesOrderDetails.SalesOrderDetails.SalesOrderDetails.LineItems != null)
            {
                logMessage.AppendLine(DateTime.Now.ToString() + " - This Order is Eligible for Service SKU");
                salesOrderRequest.eComXML = UpdateServiceSKULineItem(ref salesOrderRequest, ref logMessage, serviceSku);
                logMessage.AppendLine(DateTime.Now.ToString() + " - Ecom Service SKU Order xml: " + salesOrderRequest.eComXML);
            }

            XmlNodeList orderLines = xmlDoc.SelectNodes("/CreateEcomOrder/Orders/Order/OrderLines/OrderLine");
            foreach (XmlNode line in orderLines)
            {
                SalesLineItem lineItem = new SalesLineItem();
                lineItem.ItemNumber = line["Item"].Attributes["ItemID"].Value.Trim();
                lineItem.OrderLineId = Convert.ToInt32(line.Attributes["LineNo"].Value);
                lineItem.LineTotalAmount = Convert.ToDecimal(line["LinePriceInfo"].Attributes["PricePerSku"].Value) * Convert.ToDecimal(line.Attributes["OrderedQty"].Value);
                lineItem.OrderLineExemptionNo = "";
                lineItem.OrderedQuantity = Convert.ToDecimal(line.Attributes["OrderedQty"].Value);
                lines.Add(lineItem);
            }
            salesOrderRequest.SalesOrderEntity.SalesOrderDetails.SalesOrderDetails.LineItems = lines;

        }

        /// <summary>
        /// This method returns the attribute value in the xml.
        /// </summary>
        /// <param name="xmlString">Xml string to select the attribute value</param>
        /// <param name="element">Element name to select the attribute value</param>
        /// <param name="attribute">Attribute name to find the value</param>
        /// <param name="blRequired">Boolean value to specify whether the attribute/element is required or not</param>
        /// <returns>The value for the attribute as string</returns>
        private String GetXmlAttributeValue(XmlDocument xmlDoc, String element, String attribute, bool blRequired)
        {
            // Variable declarations.
            String strAttributeValue = String.Empty;

            // Select the node.
            XmlNode xmlNode = xmlDoc.SelectSingleNode("/" + element);

            // Check whether the node is null or not. Return the value if the node is not null.
            if (xmlNode != null)
            {
                for (int i = 0; i < xmlNode.Attributes.Count; i++)
                {
                    if (xmlNode.Attributes.Item(i).Name.Trim().Equals(attribute.Trim()))
                    {
                        strAttributeValue = xmlNode.Attributes.Item(i).Value.Trim();
                        break;
                    }
                }

                // Check whether component needs to throw the error or not at attribute level.
                if (String.IsNullOrEmpty(strAttributeValue) && blRequired)
                {
                    throw new XmlException("The attribute \"" + attribute + "\" of the \"" + element + "\" element is missing or it may not have value in the xml.");
                }
            }
            // Check whether component needs to throw the error or not at element level.
            else if (blRequired)
            {
                throw new XmlException("The \"" + element + "\" element is missing in the xml.");
            }

            // Return the value.
            return strAttributeValue.ToString().Trim();
        }

        /// <summary>
        /// To get tax user code
        /// </summary>
        /// <param name="ConnectString"></param>
        /// <param name="companyId"></param>
        /// <param name="dtOrderDtlsType"></param>
        /// <returns></returns>
        public string GetCanadianTaxEligibleDetails(string ConnectString, int companyId, DataTable dtOrderDtlsType)
        {
            string strTaxUserCode = string.Empty;
            try
            {
                ISalesOrderUpdateRepository salesDataAccess = new ChemPoint.GP.SalesOrderDL.SalesOrderDL(ConnectString);
                strTaxUserCode = salesDataAccess.GetCanadianTaxEligibleDetails(dtOrderDtlsType, companyId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return strTaxUserCode;
        }
    }
}
