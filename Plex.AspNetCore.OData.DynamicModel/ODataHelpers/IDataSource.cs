using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public interface IDataSource
{
    Task<IEdmModel> GetEdmModelAsync(string dataSource);
    Task<EdmEntityObjectCollection?> GetAsync(DataSourceParameter dataSourceParam);
    public string? DataSourceName { get; }
}
