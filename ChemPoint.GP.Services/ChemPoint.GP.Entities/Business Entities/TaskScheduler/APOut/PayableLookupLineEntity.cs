using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.APOut
{
    public class PayableLookupLineEntity
    {
        //For BusinessGroup
        private int _businessGroupId;
        private int _businessGroupCode;
        private string _businessGroupName;
        //FormatException Business Unit
        private int _businessUnitId;
        private int _businessUnitCode;
        private string _businessUnitName;
        //For Expense Type Name
        private int _expenseTypeId;
        private string _expenseTypeName;
        private string _expenseTypeShortName;
        //For Expense Report Id
        private int _expenseReportId;

        private string _ctsiId;
        //API EMEA
        private string _apiId;

        public int BusinessGroupId { get { return _businessGroupId; } set { _businessGroupId = value; } }
        public int BusinessGroupCode { get { return _businessGroupCode; } set { _businessGroupCode = value; } }
        public string BusinessGroupName { get { return _businessGroupName; } set { _businessGroupName = value; } }

        public int BusinessUnitId { get { return _businessUnitId; } set { _businessUnitId = value; } }
        public int BusinessUnitCode { get { return _businessUnitCode; } set { _businessUnitCode = value; } }
        public string BusinessUnitName { get { return _businessUnitName; } set { _businessUnitName = value; } }

        public int ExpenseTypeNameId { get { return _expenseTypeId; } set { _expenseTypeId = value; } }
        public string ExpenseTypeName { get { return _expenseTypeName; } set { _expenseTypeName = value; } }
        public string ExpenseTypeShortName { get { return _expenseTypeShortName; } set { _expenseTypeShortName = value; } }

        public int ExpenseReportId { get { return _expenseReportId; } set { _expenseReportId = value; } }

        public string CtsiId { get { return _ctsiId; } set { _ctsiId = value; } }
        #region API
        public string ApiId { get { return _apiId; } set { _apiId = value; } }
        #endregion
    }
}
