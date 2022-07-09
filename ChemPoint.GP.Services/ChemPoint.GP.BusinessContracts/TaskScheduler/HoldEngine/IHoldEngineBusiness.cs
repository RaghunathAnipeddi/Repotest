using Chempoint.GP.Model.Interactions.HoldEngine;

namespace ChemPoint.GP.BusinessContracts.TaskScheduler.HoldEngine
{
    public interface IHoldEngineBusiness 
    {
        HoldEngineResponse ProcessCreditHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessCustomerHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessCustomerDocumentHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessFirstOrderHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessSalesAlertHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessManualHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessTermHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessUpdateOpenOrdersPaymentTerms(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessTaxHold(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessCustCreditCache(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessCreditHoldEngine(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessHoldEngine(HoldEngineRequest holdEngineRequestObj);

        HoldEngineResponse ProcessVatHold(HoldEngineRequest holdEngineRequest);

        HoldEngineResponse ProcessHoldForOrder(HoldEngineRequest processHoldEngineRequestObj);

        HoldEngineResponse ProcessFreightHold(HoldEngineRequest holdEngineRequest);

        HoldEngineResponse ProcessExportHold(HoldEngineRequest exportHoldRequestObj);

        #region ExportHold

        HoldEngineResponse ExportHoldMailingDetails(HoldEngineRequest exportHoldRequestObj);

        #endregion
    }
}
