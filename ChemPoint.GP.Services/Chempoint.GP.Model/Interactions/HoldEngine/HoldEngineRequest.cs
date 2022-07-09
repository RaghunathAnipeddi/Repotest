using Chempoint.GP.Model.Interactions.Email;
using ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine;
using System.Collections.Generic;

namespace Chempoint.GP.Model.Interactions.HoldEngine
{
    public class HoldEngineRequest 
    {
        public short CompanyId { get; set; }

        public string ConnectionString { get; set; }

        public HoldEngineEntity HoldEngineEntity { get; set; }

        public List<string[]> CountryDetails { get; set; }

        public int AppConfigID { get; set; }

        public SendEmailRequest EmailRequest { get; set; }
    }
}
