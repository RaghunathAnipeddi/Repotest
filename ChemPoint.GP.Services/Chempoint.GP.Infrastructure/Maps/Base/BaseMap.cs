using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Base
{
    public abstract class BaseMap<TModel> : IDataRecordMap<TModel>
        where TModel : class, new()
    {
        public virtual TModel Map(IDataRecord dr)
        {
            var model = new TModel();
            return model;
        }
    }

    public abstract class BaseDataTableMap<TModel> : IDataRowMap<TModel>
        where TModel : class, new()
    {
        public virtual TModel Map(DataRow dr)
        {
            var model = new TModel();
            //Do your logic here
            return model;
        }
    }
}
