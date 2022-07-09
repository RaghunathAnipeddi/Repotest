using Chempoint.GP.Model.Interactions.Setup;
using ChemPoint.GP.Entities.Business_Entities.Setup;
using System.Collections.Generic;

namespace ChemPoint.GP.DataContracts.Setup
{
    public interface ISetupDetailUpdateRepository
    {
        //Tax Detail Maintenance
        void SaveTaxDetailFeildValues(SetupRequest taxDetailSetupRequest);

        void DeleteTaxDetailRecord(SetupRequest taxDetailID);

        IEnumerable<SetupEntity> FetchTaxDetailInfo(SetupRequest taxDetailSetupRequest);

        //Tax Schedule Maintenance
        void SaveTaxSchduleDetails(SetupRequest taxScheduledMaintenanceRequest);

        void DeleteTaxScheduleRecord(SetupRequest taxScheduledMaintenanceRequest);

        IEnumerable<SetupEntity> FetchTaxScheduleInfo(SetupRequest taxScheduledMaintenanceRequest);

        //Payment Terms Setup
        void SavePaymentTermCustomRecord(SetupRequest paymentTermRequest, int companyID);

        IEnumerable<SetupEntity> GetPaymentTermCustomRecord(SetupRequest paymentTermRequest, int companyID);

        void DeletePaymentTermCustomRecord(SetupRequest paymentTermRequest, int companyID);

        SetupResponse GetCalulatedDueDateByPaymentTerm(SetupRequest duedateRequest, int companyID);
    }
}
