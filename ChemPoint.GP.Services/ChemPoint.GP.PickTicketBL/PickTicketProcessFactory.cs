using Chempoint.B2B;
using System;

namespace ChemPoint.GP.PickTicketBL
{
    public enum PickTicketOperationType
    {
        CHR = 0,
        DHL = 1,
        GA = 2,
        GL = 3,
        Jacobson = 4,
        NA = 5,
        SouthernBonded = 6
    }

    public class PickTicketProcessFactory
    {
        /// <summary>
        /// Factory method to fetch the corresponding class method based on the operation type passed.
        /// </summary>
        /// <param name="operation">Type of operation performed</param>
        /// <returns></returns>
        public IWarehouseIntegrator Process(PickTicketOperationType pickTicketOperationType)
        {
            try
            {
                switch (pickTicketOperationType)
                {
                    case PickTicketOperationType.CHR:
                        return new Chempoint.B2B.CHR();
                    case PickTicketOperationType.DHL:
                        return new Chempoint.B2B.DHL();
                    case PickTicketOperationType.GA:
                        return new Chempoint.B2B.GA();
                    case PickTicketOperationType.GL:
                        return new Chempoint.B2B.GL();
                    case PickTicketOperationType.Jacobson:
                        return new Chempoint.B2B.Jacobson();
                    case PickTicketOperationType.NA:
                        return new Chempoint.B2B.NA();
                    case PickTicketOperationType.SouthernBonded:
                        return new Chempoint.B2B.SouthernBonded();
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }
    }
}
