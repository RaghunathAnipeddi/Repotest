using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.BaseEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Purchase
{
    public class XRMCreateActivityMap : BaseDataTableMap<Integration>
    {
        public override Integration Map(DataRow dr)
        {
            Integration xrmActivity = new Integration();
            int value;
            int.TryParse(dr["POActivityTypeMasterID"].ToString(), out value);
            xrmActivity.POActivityTypeMasterID = value;
            xrmActivity.Subject = dr["Subject"].ToString();
            xrmActivity.Description = dr["Description"].ToString();
            xrmActivity.DueDate = dr["DueDate"].ToString();
            xrmActivity.Priority = dr["Priority"].ToString();
            xrmActivity.RegardingEntityName = dr["RegardingEntityName"].ToString();
            xrmActivity.RegardingEntityKeyName = dr["RegardingEntityKeyName"].ToString();
            xrmActivity.RegardingEntityId = dr["RegardingEntityId"].ToString();
            xrmActivity.OwnerEntityName = dr["OwnerEntityName"].ToString();
            xrmActivity.OwnerEntityKeyName = dr["OwnerEntityKeyName"].ToString();
           // int.TryParse(dr["LineNumber"].ToString(), out value);
           // xrmActivity.LineNumber = value;
            xrmActivity.OwnerEntityId = dr["OwnerEntityId"].ToString();
          //  int.TryParse(dr["IsActivityCreatedinXRM"].ToString(), out value);
           // xrmActivity.IsActivityCreatedinXRM = value;

            return xrmActivity;
        }
    }
}
