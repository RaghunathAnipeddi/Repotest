using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChemPoint.GP.DataContracts.Base;
using ChemPoint.GP.Entities.Business_Entities;
using Chempoint.GP.Model.Interactions.TaskScheduler.HoldEngine;

namespace ChemPoint.GP.DataContracts.TaskScheduler.HoldEngine
{
    public interface IHoldEngineRepository : IRepository
    {
        object ProcessCreditHold(HoldEngineEntity Credithold, int companyID);
        object ProcessCustomerHold(HoldEngineEntity Cusomerhold, int companyID);
        object ProcessCustomerDocumentHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessFirstOrderHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessSalesAlertHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessManualHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessTermHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessUpdateOpenOrdersPaymentTerms(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessVATHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessTaxHold(HoldEngineEntity CustomerDocumenthold, int companyID);
        object ProcessCustCreditCache(HoldEngineEntity CustomerDocumenthold, int companyID);
        //HoldEngineResponse ProcessCustomerExecuteRequest(HoldEngineRequest HoldEngineRequestObj);
    }
}
