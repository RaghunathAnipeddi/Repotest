using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChemPoint.GP.Entities.BaseEntities
{
    public class Integration : IModelBase
    {
        public string Subject { get; set; }


        public string Description { get; set; }

        public string Priority { get; set; }

        public string DueDate { get; set; }

        public string RegardingEntityName { get; set; }

        public string RegardingEntityKeyName { get; set; }

        public string RegardingEntityId { get; set; }

        public string OwnerEntityName { get; set; }

        public string OwnerEntityKeyName { get; set; }

        public string OwnerEntityId { get; set; }

        public int POActivityTypeMasterID { get; set; }

        

    }
}
