using Chempoint.GP.Model.Interactions.Setup;

namespace ChemPoint.GP.BusinessContracts.Setup
{
    public interface ISetupDetailUpdate
    {
        //Tax Detail Maintenance
        SetupResponse SaveTaxDetailsToDB(SetupRequest taxDetailSetupRequest);

        SetupResponse DeleteTaxDetailsFromDB(SetupRequest taxDetailSetupRequest);

        SetupResponse FetchTaxDetailCustomRecord(SetupRequest taxDetailSetupRequest);

        //Tax Schedule Maintenance Window
        SetupResponse SaveTaxScheduleToDB(SetupRequest taxScheduledMaintenanceRequest);

        SetupResponse DeleteTaxScheduleFromDB(SetupRequest taxScheduledMaintenanceRequest);

        SetupResponse FetchTaxScheduleCustomRecord(SetupRequest taxScheduledMaintenanceRequest);

        SetupResponse SavePaymentTermDetail(SetupRequest paymentTermeRequest);

        SetupResponse DeletePaymentTermDetail(SetupRequest paymentTermRequest);

        SetupResponse FetchPaymentTermDetail(SetupRequest paymentTermRequest);

        SetupResponse GetCalulatedDueDateByPaymentTerm(SetupRequest duedateRequest, int companyId);
    }
}
