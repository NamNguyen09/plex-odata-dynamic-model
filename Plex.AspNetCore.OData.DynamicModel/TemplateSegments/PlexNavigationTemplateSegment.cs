using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.TemplateSegments;
/// <summary>
/// Reference https://github.com/OData/AspNetCoreOData/tree/main/sample/ODataDynamicModel
/// </summary>
public class PlexNavigationTemplateSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/{navigation}";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (!context.RouteValues.TryGetValue("navigation", out object? navigationNameObj))
        {
            return false;
        }

        string? navigationName = navigationNameObj as string;
        if (string.IsNullOrWhiteSpace(navigationName)) return false;
        KeySegment? keySegment = context.Segments.LastOrDefault() as KeySegment;
        if (keySegment == null) return false;
        IEdmEntityType? entityType = keySegment.EdmType as IEdmEntityType;
        if (entityType == null) return false;
        IEdmNavigationProperty? navigationProperty = entityType.NavigationProperties().FirstOrDefault(n => n.Name == navigationName);
        if (navigationProperty == null) return false;

        var navigationSource = keySegment.NavigationSource;
        IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty);

        NavigationPropertySegment seg = new(navigationProperty, targetNavigationSource);
        context.Segments.Add(seg);
        return true;
    }
}