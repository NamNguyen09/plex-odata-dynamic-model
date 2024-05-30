using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class DataSourceParameter
{
    public IEdmEntityType? EdmEntityType { get; set; }
    public IEdmCollectionType? EdmCollectionType { get; set; }
    public ODataQueryOptions? QueryOptions { get; set; }
    public string? WhereClause { get; set; }
}