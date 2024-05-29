using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Plex.AspNetCore.OData.DynamicModel.TemplateSegments;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class PlexODataRoutingApplicationModelProvider : IApplicationModelProvider
{
    private readonly PlexODataOptions _plexOptions;
    public PlexODataRoutingApplicationModelProvider(IOptions<ODataOptions> options,
                IOptions<PlexODataOptions> plexOptions)
    {
        _plexOptions = plexOptions.Value;
        options.Value.AddRouteComponents($"{_plexOptions.RoutePrefix}/{{datasource}}", EdmCoreModel.Instance).EnableQueryFeatures(100);
    }

    /// <summary>
    /// Gets the order value for determining the order of execution of providers.
    /// </summary>
    public int Order => 90;

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
        EdmModel model = new();
        string prefix = $"{_plexOptions.RoutePrefix}/{{datasource}}";
        foreach (var controllerModel in context.Result.Controllers)
        {
            if (controllerModel.ControllerName == "HandleAll")
            {
                ProcessHandleAll(prefix, model, controllerModel);
                continue;
            }

            if (controllerModel.ControllerName == "Metadata")
            {
                ProcessMetadata(prefix, model, controllerModel);
                continue;
            }
        }
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
    }

    private void ProcessHandleAll(string prefix, IEdmModel model, ControllerModel controllerModel)
    {
        foreach (var actionModel in controllerModel.Actions)
        {
            if (actionModel.ActionName == "GetNavigation")
            {
                ODataPathTemplate path = new(new PlexEntitySetTemplateSegment(),
                                             new PlexEntitySetWithKeyTemplateSegment(),
                                             new PlexNavigationTemplateSegment());

                actionModel.AddSelector("get", prefix, model, path);
            }
            else if (actionModel.ActionName == "GetName")
            {
                ODataPathTemplate path = new(new PlexEntitySetTemplateSegment(),
                                             new PlexEntitySetWithKeyTemplateSegment(),
                                             new StaticNameSegment());

                actionModel.AddSelector("get", prefix, model, path);
            }
            else if (actionModel.ActionName == "Get")
            {
                if (actionModel.Parameters.Count == 1)
                {
                    ODataPathTemplate path = new(new PlexEntitySetTemplateSegment());
                    actionModel.AddSelector("get", prefix, model, path);
                }
                else
                {
                    ODataPathTemplate path = new(new PlexEntitySetTemplateSegment(), new PlexEntitySetWithKeyTemplateSegment());
                    actionModel.AddSelector("get", prefix, model, path);
                }
            }
        }
    }

    private void ProcessMetadata(string prefix, IEdmModel model, ControllerModel controllerModel)
    {
        foreach (var actionModel in controllerModel.Actions)
        {
            if (actionModel.ActionName == "GetMetadata")
            {
                ODataPathTemplate path = new(MetadataSegmentTemplate.Instance);
                actionModel.AddSelector("get", prefix, model, path);
            }
            else if (actionModel.ActionName == "GetServiceDocument")
            {
                ODataPathTemplate path = [];
                actionModel.AddSelector("get", prefix, model, path);
            }
        }
    }
}
