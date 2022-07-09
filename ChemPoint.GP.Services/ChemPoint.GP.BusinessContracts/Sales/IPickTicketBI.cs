using Chempoint.GP.Model.Interactions.Sales;

namespace ChemPoint.GP.BusinessContracts.Sales
{
    public interface IPickTicketBI
    {
        PickTicketResponse PrintPickTicket(PickTicketRequest pickTicketRequest);

        PickTicketResponse GetOrderPickTicketEligibleDetails(PickTicketRequest pickTicketRequest);
    }
}
