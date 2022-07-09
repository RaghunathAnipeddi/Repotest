using Chempoint.GP.Infrastructure.Maps.Base;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chempoint.GP.Infrastructure.Maps.Purchase
{
    /// <PayableManagement>
    /// Project Name        :   payable mangement migration
    /// Affected Module     :   Purchase
    /// Affected Windows    :   Mapping CTSI Validation SP details to Object.
    /// Developed on        :   April2017
    /// Developed by        :   Muthu
    ///-------------------------------------------------------------------------------------------
    ///     Date		Author          Description
    ///-------------------------------------------------------------------------------------------
    /// **Apr2017       Mthangaraj      Initial Creation.
    /// </PayableManagement>
    public class PayableManagementCTSITaxMap : BaseDataTableMap<PayableLineEntity>
    {
        public override PayableLineEntity Map(DataRow dr)
        {
            PayableLineEntity payableEntity = new PayableLineEntity();
            decimal decimalResult;
            payableEntity.BaseCharge = dr["BaseCharge"].ToString();
            payableEntity.TaxScheduleId = dr["TaxScheduleId"].ToString();
            payableEntity.TaxDetailId = dr["TaxDetailId"].ToString();
            decimal.TryParse(dr["TaxPercentage"].ToString(), out decimalResult);
            payableEntity.TaxPercentage = decimalResult;
            payableEntity.TaxDetailIdDescription = dr["TaxDetailIdDescription"].ToString();
            payableEntity.CurrencyCode = dr["Currency"].ToString();

            return payableEntity;
        }
    }
}
