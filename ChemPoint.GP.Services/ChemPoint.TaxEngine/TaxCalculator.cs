using System;
using Avalara.AvaTax.Adapter.AddressService;
using Avalara.AvaTax.Adapter.TaxService;

namespace ChemPoint.TaxEngine
{
    /// <summary>
    /// Class which calculates the tax. This makes a web service call to the avalara get tax details.
    /// </summary>
    public class TaxCalculator
    {
        /// <summary>
        /// Method which calculates the tax outside GP and returns the tax details.
        /// </summary>
        /// <param name="objTaxRequest"></param>
        /// <returns></returns>
        public TaxResult CalculateTax(TaxRequest objTaxRequest)
        {
            try
            {
                // creates a new instance of Tax service adaptor
                TaxSvc taxSvc = new TaxSvc();

                // configures the connection strings
                ConfigureConnections(taxSvc);

                // makes the GetTaxRequest instance from objTaxRequest
                GetTaxRequest getTaxRequest = SetupGetTaxRequest(objTaxRequest);

                // Calls the avalara adaptor which calculates the tax
                GetTaxResult getTaxResult = taxSvc.GetTax(getTaxRequest);

                // makes the TaxResult instance from GetTaxResult
                TaxResult objTaxResult = SetupTaxResult(getTaxResult);

                // returns
                return objTaxResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Method which converts the custom TaxRequest object to avalara GetTaxRequest object
        /// </summary>
        /// <param name="objTaxRequest"></param>
        /// <returns></returns>
        private GetTaxRequest SetupGetTaxRequest(TaxRequest objTaxRequest)
        {
            try
            {
                // creates a new taxRequest instance
                GetTaxRequest taxRequest = new GetTaxRequest();

                // assigns the header details
                taxRequest.CompanyCode = objTaxRequest.CmpyCode.ToString();
                taxRequest.ExemptionNo = objTaxRequest.CustomerExemptionNo;
                taxRequest.CustomerCode = objTaxRequest.CustomerNumber;
                taxRequest.CustomerUsageType = objTaxRequest.CustomerUseCode;
                taxRequest.DocDate = objTaxRequest.DocumentDate;
                taxRequest.DocCode = objTaxRequest.DocumentNumber;
                taxRequest.CurrencyCode = objTaxRequest.OrderCurrencyCode.ToString();

                if (objTaxRequest.LineDetails.Length > 0)
                {
                    // shipfrom address
                    Address shipFromAddress = new Address();
                    shipFromAddress.Line1 = objTaxRequest.LineDetails[0].ShipFromAddressLine1;
                    shipFromAddress.Line2 = objTaxRequest.LineDetails[0].ShipFromAddressLine2;
                    shipFromAddress.Line3 = objTaxRequest.LineDetails[0].ShipFromAddressLine3;
                    shipFromAddress.City = objTaxRequest.LineDetails[0].ShipFromAddressCity;
                    shipFromAddress.Region = objTaxRequest.LineDetails[0].ShipFromAddressState;
                    shipFromAddress.PostalCode = objTaxRequest.LineDetails[0].ShipFromAddressZip;
                    shipFromAddress.Country = objTaxRequest.LineDetails[0].ShipFromAddressCountry;
                    taxRequest.OriginAddress = shipFromAddress;

                    // ship to address
                    Address shipToAddress = new Address();
                    shipToAddress.Line1 = objTaxRequest.LineDetails[0].ShipToAddressLine1;
                    shipToAddress.Line2 = objTaxRequest.LineDetails[0].ShipToAddressLine2;
                    shipToAddress.Line3 = objTaxRequest.LineDetails[0].ShipToAddressLine3;
                    shipToAddress.City = objTaxRequest.LineDetails[0].ShipToAddressCity;
                    shipToAddress.Region = objTaxRequest.LineDetails[0].ShipToAddressState;
                    shipToAddress.PostalCode = objTaxRequest.LineDetails[0].ShipToAddressZip;
                    shipToAddress.Country = objTaxRequest.LineDetails[0].ShipToAddressCountry;
                    taxRequest.DestinationAddress = shipToAddress;
                }

                taxRequest.DocType = DocumentType.SalesOrder;
                taxRequest.DetailLevel = DetailLevel.Line;
                taxRequest.TaxOverride.TaxOverrideType = TaxOverrideType.None;

                // assigns the line details
                for (int i = 0; i < objTaxRequest.LineDetails.Length; i++)
                {
                    // creates the new line
                    Line line = new Line();
                    line.ItemCode = objTaxRequest.LineDetails[i].ItemNumber;
                    line.ExemptionNo = objTaxRequest.LineDetails[i].LineExemptionNo;
                    line.Amount = objTaxRequest.LineDetails[i].LineExtendedPrice;
                    line.Qty = objTaxRequest.LineDetails[i].LineQty;
                    line.No = objTaxRequest.LineDetails[i].LineSeq.ToString();
                    line.TaxCode = "AVATAX";
                    line.TaxOverride.TaxOverrideType = TaxOverrideType.None;

                    // shipfrom address
                    Address lineShipFromAddress = new Address();
                    lineShipFromAddress.Line1 = objTaxRequest.LineDetails[i].ShipFromAddressLine1;
                    lineShipFromAddress.Line2 = objTaxRequest.LineDetails[i].ShipFromAddressLine2;
                    lineShipFromAddress.Line3 = objTaxRequest.LineDetails[i].ShipFromAddressLine3;
                    lineShipFromAddress.City = objTaxRequest.LineDetails[i].ShipFromAddressCity;
                    lineShipFromAddress.Region = objTaxRequest.LineDetails[i].ShipFromAddressState;
                    lineShipFromAddress.PostalCode = objTaxRequest.LineDetails[i].ShipFromAddressZip;
                    lineShipFromAddress.Country = objTaxRequest.LineDetails[i].ShipFromAddressCountry;
                    line.OriginAddress = lineShipFromAddress;

                    // shipfrom address
                    if (objTaxRequest.LineDetails[i].LineShippingType == LineShippingType.Pickup)
                        line.DestinationAddress = lineShipFromAddress;
                    else
                    {
                        Address lineShipToAddress = new Address();
                        lineShipToAddress.Line1 = objTaxRequest.LineDetails[i].ShipToAddressLine1;
                        lineShipToAddress.Line2 = objTaxRequest.LineDetails[i].ShipToAddressLine2;
                        lineShipToAddress.Line3 = objTaxRequest.LineDetails[i].ShipToAddressLine3;
                        lineShipToAddress.City = objTaxRequest.LineDetails[i].ShipToAddressCity;
                        lineShipToAddress.Region = objTaxRequest.LineDetails[i].ShipToAddressState;
                        lineShipToAddress.PostalCode = objTaxRequest.LineDetails[i].ShipToAddressZip;
                        lineShipToAddress.Country = objTaxRequest.LineDetails[i].ShipToAddressCountry;
                        line.DestinationAddress = lineShipToAddress;
                    }

                    // adds a new line
                    taxRequest.Lines.Add(line);
                }

                // returns
                return taxRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        /// <summary>
        /// Method which converts the custom TaxResult object to avalara GetTaxResult object
        /// </summary>
        /// <param name="getTaxResult"></param>
        /// <returns></returns>
        private TaxResult SetupTaxResult(GetTaxResult getTaxResult)
        {
            // creates a new taxResult instance
            TaxResult objTaxResult = new TaxResult();
            // assigns the header details
            objTaxResult.DocStatus = getTaxResult.DocStatus.ToString();
            objTaxResult.DocType = getTaxResult.DocType.ToString();
            objTaxResult.Timestamp = getTaxResult.Timestamp;
            objTaxResult.TotalAmount = getTaxResult.TotalAmount;
            objTaxResult.TotalExemption = getTaxResult.TotalExemption;
            objTaxResult.TotalTaxable = getTaxResult.TotalTaxable;

            if (getTaxResult.TaxLines.Count > 0)
            {
                objTaxResult.FrieghtTaxRate = (getTaxResult.TaxLines[0].Tax > 0 ? getTaxResult.TaxLines[0].Rate : 0);
                objTaxResult.MiscTaxRate = (getTaxResult.TaxLines[0].Tax > 0 ? getTaxResult.TaxLines[0].Rate : 0);
            }
            // assigns the line details
            foreach (TaxLine taxLine in getTaxResult.TaxLines)
            {
                // creates the new line
                TaxLineDetail taxLineDetail = new TaxLineDetail();
                taxLineDetail.BoundaryLevel = taxLine.BoundaryLevel.ToString();
                taxLineDetail.Discount = taxLine.Discount;
                taxLineDetail.ExemptCertId = taxLine.ExemptCertId;
                taxLineDetail.Exemption = taxLine.Exemption;
                taxLineDetail.LineSeq = taxLine.No;
                taxLineDetail.Rate = taxLine.Rate;
                taxLineDetail.Tax = taxLine.Tax;
                taxLineDetail.Taxability = taxLine.Taxability;
                taxLineDetail.TaxableAmt = taxLine.Taxable;
                taxLineDetail.TaxCode = taxLine.TaxCode;

                // adds a new line
                objTaxResult.AddTaxLineDetail(taxLineDetail);
            }

            // returns
            return objTaxResult;
        }

        /// <summary>
        /// This method configures all the web service connections along with licenses.
        /// </summary>
        /// <param name="taxSvc"></param>
        private void ConfigureConnections(TaxSvc taxSvc)
        {
            // general profile
            taxSvc.Profile.Client = "ChemPoint Tax Engine";
            
            // connection strings
            taxSvc.Configuration.Url = TaxEngine.URL;
            taxSvc.Configuration.Security.Account = TaxEngine.ACCOUNT;
            taxSvc.Configuration.Security.License = TaxEngine.REGKEY;
            taxSvc.Configuration.Security.UserName = TaxEngine.USERNAME;
            taxSvc.Configuration.Security.Password = TaxEngine.PASSWORD;		
        }
    }
}
