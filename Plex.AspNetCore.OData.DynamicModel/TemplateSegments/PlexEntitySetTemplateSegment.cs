using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.TemplateSegments;
public class PlexEntitySetTemplateSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/{entityset}";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        if (!context.RouteValues.TryGetValue("entityset", out object? classname))
        {
            return false;
        }

        string? entitySetName = classname as string;
        if (string.IsNullOrWhiteSpace(entitySetName)) return false;

        // if you want to support case-insensitive
        var edmEntitySet = context.Model.EntityContainer.EntitySets()
                                        .FirstOrDefault(e => string.Equals(entitySetName, e.Name, StringComparison.OrdinalIgnoreCase));

        //var edmEntitySet = context.Model.EntityContainer.FindEntitySet(entitySetName);
        if (edmEntitySet == null) return false;

        EntitySetSegment segment = new(edmEntitySet);
        context.Segments.Add(segment);
        return true;
    }
}