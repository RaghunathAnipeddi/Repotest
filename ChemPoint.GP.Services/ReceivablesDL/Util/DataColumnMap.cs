using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivablesDL.Util
{
    public class DataColumnMap
    {
        public string ColumnName { get; set; }

        public Type Type { get; set; }

        public bool IsDefaultValue { get; set; }

        public object DefaultValue { get; set; }
    }
}
