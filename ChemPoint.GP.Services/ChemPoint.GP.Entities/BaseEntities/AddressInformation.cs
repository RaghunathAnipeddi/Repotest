using System;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class AddressInformation : IModelBase
    {
        public int SopType { get; set; }

        public string SopNumber { get; set; }

        public string CustomerID { get; set; }

        public string CustomerName { get; set; }

        public string CustomerGuid { get; set; }

        public int AddressId { get; set; }

        public string AddressCode { get; set; }

        public string ContactName { get; set; }

        public string ShipToName { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public string Phone1 { get; set; }

        public string Phone2 { get; set; }

        public string Phone3 { get; set; }

        public string Fax { get; set; }

        public string CountryCode { get; set; }

        public string Notes { get; set; }
    }
}
