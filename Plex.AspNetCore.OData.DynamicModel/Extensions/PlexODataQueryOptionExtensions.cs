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
            query = string.Format("{0} OFFSET {1} ROWS ", query, queryOptions.Skip.Value);
        }
        else
        {
            query = string.Format("{0} OFFSET {1} ROWS ", query, 0);
        }

        //Apply $top
        if (queryOptions.Top != null)
        {
            var top = queryOptions.Top.Value > plexODataOptions.MaxPageSize ? plexODataOptions.MaxPageSize + 1 : queryOptions.Top.Value;
            query = string.Format("{0} FETCH NEXT {1} ROWS ONLY; ", query, top);
        }
        else
        {
            query = string.Format("{0} FETCH NEXT {1} ROWS ONLY; ", query, plexODataOptions.MaxPageSize + 1);
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
            query = string.Format("{0} {1} ", query, orderBy);
        }
        else
        {
            IEdmProperty? defaultOrderByColumn = edmEntityType.DeclaredProperties.FirstOrDefault();
            if (defaultOrderByColumn != null) query = string.Format("{0} order by {1} asc ", query, defaultOrderByColumn.Name);
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
            query = string.Format("{0} AND {1} ", query, whereClause);
        }
        else
        {
            query = string.Format("{0} where {1} ", query, whereClause);
        }

        return query;
    }
}
