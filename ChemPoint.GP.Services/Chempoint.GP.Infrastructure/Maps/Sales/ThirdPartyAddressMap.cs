using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class ThirdPartyAddressMap : BaseDataTableMap<AddressInformation>
    {
        public override AddressInformation Map(DataRow dr)
        {
            AddressInformation addressInformation = new AddressInformation();

            int value;
            int.TryParse(dr["CustomAddressID"].ToString(), out value);
            addressInformation.AddressId = value;
            addressInformation.SopNumber = dr["SopNumber"].ToString();
            int.TryParse(dr["SopType"].ToString(), out value);
            addressInformation.SopType = value;
            addressInformation.ContactName = dr["ContactPerson"].ToString();
            addressInformation.AddressLine1 = dr["Address1"].ToString();
            addressInformation.AddressLine2 = dr["Address2"].ToString();
            addressInformation.AddressLine3 = dr["Address3"].ToString();
            addressInformation.City = dr["City"].ToString();
            addressInformation.State = dr["State"].ToString();
            addressInformation.ZipCode = dr["Zip"].ToString();
            addressInformation.Country = dr["Country"].ToString();
            return addressInformation;
        }
    }
}
