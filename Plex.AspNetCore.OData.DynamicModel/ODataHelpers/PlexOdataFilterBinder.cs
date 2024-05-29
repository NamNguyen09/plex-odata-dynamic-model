using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class PlexOdataFilterBinder
{
    public string? BindFilter(FilterQueryOption filterQuery)
    {
        return BindFilterClause(filterQuery.FilterClause);
    }

    public string? BindFilterClause(FilterClause filterClause)
    {
        return Bind(filterClause.Expression);
    }

    public string? Bind(QueryNode node)
    {
        CollectionNode? collectionNode = node as CollectionNode;
        SingleValueNode? singleValueNode = node as SingleValueNode;

        if (collectionNode != null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.CollectionNavigationNode:
                    CollectionNavigationNode? navigationNode = node as CollectionNavigationNode;
                    return BindNavigationPropertyNode(navigationNode?.Source, navigationNode?.NavigationProperty);

                case QueryNodeKind.CollectionPropertyAccess:
                    return BindCollectionPropertyAccessNode(node as CollectionPropertyAccessNode);
            }
        }
        else if (singleValueNode != null)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    return BindBinaryOperatorNode(node as BinaryOperatorNode);

                case QueryNodeKind.Constant:
                    return BindConstantNode(node as ConstantNode);

                case QueryNodeKind.Convert:
                    return BindConvertNode(node as ConvertNode);

                case QueryNodeKind.ResourceRangeVariableReference://.EntityRangeVariableReference:
                    return BindRangeVariable((node as ResourceRangeVariableReferenceNode)?.RangeVariable);

                case QueryNodeKind.NonResourceRangeVariableReference:
                    return BindRangeVariable((node as NonResourceRangeVariableReferenceNode)?.RangeVariable);

                case QueryNodeKind.SingleValuePropertyAccess:
                    return BindPropertyAccessQueryNode(node as SingleValuePropertyAccessNode);

                case QueryNodeKind.UnaryOperator:
                    return BindUnaryOperatorNode(node as UnaryOperatorNode);

                case QueryNodeKind.SingleValueFunctionCall:
                    return BindSingleValueFunctionCallNode(node as SingleValueFunctionCallNode);

                case QueryNodeKind.SingleNavigationNode:
                    SingleNavigationNode? navigationNode = node as SingleNavigationNode;
                    return BindNavigationPropertyNode(navigationNode?.Source, navigationNode?.NavigationProperty);

                case QueryNodeKind.Any:
                    return BindAnyNode(node as AnyNode);

                case QueryNodeKind.All:
                    return BindAllNode(node as AllNode);
            }
        }

        throw new NotSupportedException(string.Format("Nodes of type {0} are not supported", node.Kind));
    }

    string? BindCollectionPropertyAccessNode(CollectionPropertyAccessNode? collectionPropertyAccessNode)
    {
        if (collectionPropertyAccessNode == null) return null;
        return Bind(collectionPropertyAccessNode.Source) + "." + collectionPropertyAccessNode.Property.Name;
    }

    string? BindNavigationPropertyNode(SingleValueNode? singleValueNode, IEdmNavigationProperty? edmNavigationProperty)
    {
        if (singleValueNode == null || edmNavigationProperty == null) return null;
        return Bind(singleValueNode) + "." + edmNavigationProperty.Name;
    }

    string? BindAllNode(AllNode? allNode)
    {
        if (allNode == null) return null;
        string innerQuery = "not exists ( from " + Bind(allNode.Source) + " " + allNode.RangeVariables.First().Name;
        innerQuery += " where NOT(" + Bind(allNode.Body) + ")";
        return innerQuery + ")";
    }

    string? BindAnyNode(AnyNode? anyNode)
    {
        if (anyNode == null) return null;
        string innerQuery = "exists ( from " + Bind(anyNode.Source) + " " + anyNode.RangeVariables.First().Name;
        if (anyNode.Body != null)
        {
            innerQuery += " where " + Bind(anyNode.Body);
        }
        return innerQuery + ")";
    }

    string? BindNavigationPropertyNode(SingleEntityNode singleEntityNode, IEdmNavigationProperty edmNavigationProperty)
    {
        return Bind(singleEntityNode) + "." + edmNavigationProperty.Name;
    }

    string? BindSingleValueFunctionCallNode(SingleValueFunctionCallNode? singleValueFunctionCallNode)
    {
        if (singleValueFunctionCallNode == null) return null;
        var arguments = singleValueFunctionCallNode.Parameters.ToList();
        switch (singleValueFunctionCallNode.Name)
        {
            case "cast":
                return singleValueFunctionCallNode.Name + "('" + Bind(arguments[0]) + "' AS DATETIME2 )";
            case "concat":
                return singleValueFunctionCallNode.Name + "(" + Bind(arguments[0]) + "," + Bind(arguments[1]) + ")";

            case "length":
            case "trim":
            case "year":
            case "years":
            case "month":
            case "months":
            case "day":
            case "days":
            case "hour":
            case "hours":
            case "minute":
            case "minutes":
            case "second":
            case "seconds":
            case "round":
            case "floor":
            case "ceiling":
                return singleValueFunctionCallNode.Name + "(" + Bind(arguments[0]) + ")";
            default:
                throw new NotImplementedException();
        }
    }

    string? BindUnaryOperatorNode(UnaryOperatorNode? unaryOperatorNode)
    {
        if (unaryOperatorNode == null) return null;
        return ToString(unaryOperatorNode.OperatorKind) + "(" + Bind(unaryOperatorNode.Operand) + ")";
    }

    string? BindPropertyAccessQueryNode(SingleValuePropertyAccessNode? singleValuePropertyAccessNode)
    {
        if (singleValuePropertyAccessNode == null) return null;
        return singleValuePropertyAccessNode.Property.Name;
    }

    string? BindRangeVariable(NonResourceRangeVariable? nonentityRangeVariable)
    {
        if (nonentityRangeVariable == null) return null;
        return nonentityRangeVariable.Name.ToString();
    }

    string? BindRangeVariable(ResourceRangeVariable? entityRangeVariable)
    {
        if (entityRangeVariable == null) return null;
        return entityRangeVariable.Name.ToString();
    }

    string? BindConvertNode(ConvertNode? convertNode)
    {
        if (convertNode == null) return null;
        return Bind(convertNode.Source);
    }

    string? BindConstantNode(ConstantNode? constantNode)
    {
        if (constantNode == null) return null;
        //_positionalParmeters.Add();
        //return "?";
        if (constantNode.TypeReference.IsDateTimeOffset())
        {
            var dateTimeOffsetValue = (DateTimeOffset)constantNode.Value;
            string sqlFormattedDate = dateTimeOffsetValue.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
            return sqlFormattedDate;
        }
        return constantNode.Value.ToString();
    }

    string? BindBinaryOperatorNode(BinaryOperatorNode? binaryOperatorNode)
    {
        if (binaryOperatorNode == null) return null;
        var left = Bind(binaryOperatorNode.Left);
        var right = Bind(binaryOperatorNode.Right);
        if (binaryOperatorNode.Right.Kind != QueryNodeKind.SingleValueFunctionCall &&
            (binaryOperatorNode.Left.TypeReference.IsString() ||
            binaryOperatorNode.Left.TypeReference.IsDateTimeOffset()))
        {
            return "(" + left + " " + ToString(binaryOperatorNode.OperatorKind) + " '" + right + "')";
        }
        return "(" + left + " " + ToString(binaryOperatorNode.OperatorKind) + " " + right + ")";
    }

    string? ToString(BinaryOperatorKind? binaryOpertor)
    {
        if (binaryOpertor == null) return null;
        switch (binaryOpertor)
        {
            case BinaryOperatorKind.Add:
                return "+";
            case BinaryOperatorKind.And:
                return "AND";
            case BinaryOperatorKind.Divide:
                return "/";
            case BinaryOperatorKind.Equal:
                return "=";
            case BinaryOperatorKind.GreaterThan:
                return ">";
            case BinaryOperatorKind.GreaterThanOrEqual:
                return ">=";
            case BinaryOperatorKind.LessThan:
                return "<";
            case BinaryOperatorKind.LessThanOrEqual:
                return "<=";
            case BinaryOperatorKind.Modulo:
                return "%";
            case BinaryOperatorKind.Multiply:
                return "*";
            case BinaryOperatorKind.NotEqual:
                return "!=";
            case BinaryOperatorKind.Or:
                return "OR";
            case BinaryOperatorKind.Subtract:
                return "-";
            default:
                return null;
        }
    }

    string? ToString(UnaryOperatorKind? unaryOperator)
    {
        if (unaryOperator == null) return null;
        switch (unaryOperator)
        {
            case UnaryOperatorKind.Negate:
                return "!";
            case UnaryOperatorKind.Not:
                return "NOT";
            default:
                return null;
        }
    }
}