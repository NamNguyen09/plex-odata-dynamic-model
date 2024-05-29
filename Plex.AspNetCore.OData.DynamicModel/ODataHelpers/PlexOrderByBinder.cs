using System.Text;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class PlexOrderByBinder
{
    public string BindOrderByQueryOption(OrderByQueryOption orderByQuery)
    {
        StringBuilder sb = new();

        if (orderByQuery != null)
        {
            sb.Append("order by ");
            foreach (var orderByNode in orderByQuery.OrderByNodes)
            {
                OrderByPropertyNode? orderByPropertyNode = orderByNode as OrderByPropertyNode;

                if (orderByPropertyNode == null) throw new ODataException("Only ordering by properties is supported");

                sb.Append(orderByPropertyNode.Property.Name);
                sb.Append(orderByPropertyNode.Direction == OrderByDirection.Ascending ? " asc," : " desc,");
            }
        }

        if (sb.Length > 0 && sb[sb.Length - 1] == ',')
        {
            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }
}