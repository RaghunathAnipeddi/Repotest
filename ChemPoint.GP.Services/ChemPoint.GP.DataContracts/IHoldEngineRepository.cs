using Chempoint.GP.Model.Interactions.HoldEngine;
using ChemPoint.GP.DataContracts.Base;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;

namespace ChemPoint.GP.DataContracts.TaskScheduler.HoldEngine
{
    public interface IHoldEngineRepository : IRepository
    {
        object ProcessCreditHold(HoldEngineEntity credithold, int companyID);

        object ProcessCustomerHold(HoldEngineEntity cusomerhold, int companyID);

        object ProcessCustomerDocumentHold(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessFirstOrderHold(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessSalesAlertHold(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessManualHold(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessTermHold(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessUpdateOpenOrdersPaymentTerms(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessCustCreditCache(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessCreditHoldEngine(HoldEngineEntity customerDocumenthold, int companyID);

        object ProcessHoldEngine();

        object ProcessTaxHold(HoldEngineEntity holdEngineEntity, int companyID);

        object ProcessVatHold(HoldEngineEntity holdEngineEntity);

        object ProcessHoldForOrder(HoldEngineEntity holdEngineEntity, int companyID);
        object ProcessFreightHold(HoldEngineEntity holdEngineEntity, int companyID);
        HoldEngineResponse ProcessExportHold(HoldEngineEntity holdEngineEntity, int companyID);
    }
}
