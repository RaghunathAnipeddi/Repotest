﻿using System;
using System.Linq;

namespace ChemPoint.GP.AccountDL.Util
{
    public class DataColumnMap
    {
        public string ColumnName { get; set; }

        public Type Type { get; set; }

        public bool IsDefaultValue { get; set; }

        public object DefaultValue { get; set; }
    }
}
