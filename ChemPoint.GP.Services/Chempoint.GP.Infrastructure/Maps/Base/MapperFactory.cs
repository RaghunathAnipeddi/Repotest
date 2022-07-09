using System;

namespace Chempoint.GP.Infrastructure.Maps.Base
{
    public static class MapperFactory<TModel, TMap>
        where TMap : class, IMap, new()
    {
        public static TMap Mapper()
        {
            return new TMap();
        }
    }
}
