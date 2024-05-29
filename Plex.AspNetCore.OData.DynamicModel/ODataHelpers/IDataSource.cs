using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public interface IDataSource
{
    IEdmModel GetEdmModel(string dataSource);
    void Get(IEdmEntityTypeReference entityType, EdmEntityObjectCollection collection,
                ODataQueryContext queryContext, ODataQueryOptions queryOptions,
                PlexODataOptions plexODataOptions);
}
