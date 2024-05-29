using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.TemplateSegments;
public class StaticNameSegment : ODataSegmentTemplate
{
    public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
    {
        yield return "/Name";
    }

    public override bool TryTranslate(ODataTemplateTranslateContext context)
    {
        KeySegment? keySegment = context.Segments.LastOrDefault() as KeySegment;
        if (keySegment == null) return false;
        IEdmEntityType? entityType = keySegment.EdmType as IEdmEntityType;
        if (entityType == null) return false;
        IEdmProperty? edmProperty = entityType.Properties().FirstOrDefault(p => p.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase));
        if (edmProperty == null) return false;

        PropertySegment seg = new(edmProperty as IEdmStructuralProperty);
        context.Segments.Add(seg);
        return true;
    }
}