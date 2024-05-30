using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Plex.AspNetCore.OData.DynamicModel.ODataHelpers;

namespace Plex.AspNetCore.OData.DynamicModel.Extensions;
public static class PlexODataQueryOptionExtensions
{
    public static string ApplyTo(this ODataQueryOptions queryOptions,
                                PlexODataOptions plexODataOptions,
                                string query,
                                IEdmEntityType edmEntityType,
                                bool hasWhereClause)
    {
        //Convert $filter to SQL query
        query = queryOptions.Filter.ToFilterQuery(query, hasWhereClause);

        // convert $orderby to SQL query
        query = queryOptions.OrderBy.ToOrderByQuery(query, edmEntityType);

        //Apply $skip
        if (queryOptions.Skip != null)
        {
            query += $" OFFSET {queryOptions.Skip.Value} ROWS ";
        }
        else
        {
            query += " OFFSET 0 ROWS ";
        }

        //Apply $top
        if (queryOptions.Top != null)
        {
            var top = queryOptions.Top.Value > plexODataOptions.MaxPageSize ? plexODataOptions.MaxPageSize + 1 : queryOptions.Top.Value;
            query += $" FETCH NEXT {top} ROWS ONLY; ";
        }
        else
        {
            query += $" FETCH NEXT {plexODataOptions.MaxPageSize + 1} ROWS ONLY; ";
        }
        return query;
    }

    static string ToOrderByQuery(this OrderByQueryOption orderByQuery, string query,
                                 IEdmEntityType edmEntityType)
    {
        if (orderByQuery != null)
        {
            var orderByBinder = new PlexOrderByBinder();
            string orderBy = orderByBinder.BindOrderByQueryOption(orderByQuery);
            query += $" {orderBy}";
        }
        else
        {
            IEdmProperty? defaultOrderByColumn = edmEntityType.DeclaredProperties.FirstOrDefault();
            if (defaultOrderByColumn != null) query += $" ORDER BY {defaultOrderByColumn.Name} ASC ";
        }

        return query;
    }

    static string ToFilterQuery(this FilterQueryOption filterQuery, string query, bool hasWhereClause)
    {
        if (filterQuery == null) return query;

        var odataFilterBinder = new PlexOdataFilterBinder();
        var whereClause = odataFilterBinder.BindFilter(filterQuery);
        if (hasWhereClause)
        {
            query += $" AND {whereClause} ";
        }
        else
        {
            query += $" WHERE {whereClause} ";
        }

        return query;
    }
}
