using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Sales
{
    public class CustomerDetailAddressIDMap : BaseDataTableMap<AddressInformation>
    {
        public override AddressInformation Map(DataRow dr)
        {
            AddressInformation addressInformation = new AddressInformation();
            
            #region AddressID
            
            int value;
            addressInformation.SopNumber = dr["SopNumber"].ToString();
            int.TryParse(dr["SopType"].ToString(), out value);
            addressInformation.SopType = value;
            addressInformation.CustomerID = dr["CustomerID"].ToString();
            addressInformation.CustomerName = dr["CustomerName"].ToString();
            int.TryParse(dr["AddressTypeID"].ToString(), out value);
            addressInformation.AddressId = value;
            addressInformation.AddressCode = dr["AddressCode"].ToString();
            addressInformation.ShipToName = dr["ShipToName"].ToString();
            addressInformation.ContactName = dr["ContactPerson"].ToString();
            addressInformation.AddressLine1 = dr["Address1"].ToString();
            addressInformation.AddressLine2 = dr["Address2"].ToString();
            addressInformation.AddressLine3 = dr["Address3"].ToString();
            addressInformation.City = dr["City"].ToString();
            addressInformation.State = dr["State"].ToString();
            addressInformation.ZipCode = dr["Zip"].ToString();
            addressInformation.Country = dr["Country"].ToString();
            addressInformation.CountryCode = dr["CCode"].ToString();
            addressInformation.Phone1 = dr["Phone1"].ToString();
            //addressInformation.Phone2 = dr["Phone2"].ToString();
            //addressInformation.Phone3 = dr["Phone3"].ToString();
            addressInformation.Fax = dr["Fax"].ToString();
            return addressInformation;
    
            #endregion AddressID
        }
    }
}
