using ChemPoint.GP.Entities.BaseEntities;

namespace ChemPoint.GP.Entities.Business_Entities.TaskScheduler.HoldEngine
{
    public class HoldEngineEntity : IModelBase
    {
        public OrderHeader OrderHeader { get; set; }

        public CustomerInformation CustomerInformation { get; set; }

        public string BatchUserID { get; set; }
    }
}
