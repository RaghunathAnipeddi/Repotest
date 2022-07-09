using System.Data;

namespace Chempoint.GP.Infrastructure.Maps.Base
{
    public interface IMap
    {
    }

    public interface IDataRecordMap<T> : IMap
    {
        T Map(IDataRecord dataRecord);
    }

    public interface IDataRowMap<T> : IMap
    {
        T Map(DataRow dataRow);
    }
}
