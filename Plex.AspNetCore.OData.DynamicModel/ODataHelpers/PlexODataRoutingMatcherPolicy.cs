using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class PlexODataRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    private readonly IODataTemplateTranslator _translator;
    private readonly IDataSource _dataSource;
    public PlexODataRoutingMatcherPolicy(IODataTemplateTranslator translator,
                                         IDataSource dataSource)
    {
        _translator = translator;
        _dataSource = dataSource;
    }

    /// <summary>
    /// Gets a value that determines the order of this policy.
    /// </summary>
    public override int Order => 900 - 1;

    public string? DataSourceName { get; }

    /// <summary>
    /// Returns a value that indicates whether the matcher applies to any endpoint in endpoints.
    /// </summary>
    /// <param name="endpoints">The set of candidate values.</param>
    /// <returns>true if the policy applies to any endpoint in endpoints, otherwise false.</returns>
    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        return endpoints.Any(e => e.Metadata.OfType<ODataRoutingMetadata>().FirstOrDefault() != null);
    }

    /// <summary>
    /// Applies the policy to the CandidateSet.
    /// </summary>
    /// <param name="httpContext">The context associated with the current request.</param>
    /// <param name="candidates">The CandidateSet.</param>
    /// <returns>The task.</returns>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        IODataFeature odataFeature = httpContext.ODataFeature();
        if (odataFeature.Path != null)
        {
            // If we have the OData path setting, it means there's some Policy working.
            // Let's skip this default OData matcher policy.
            return Task.CompletedTask;
        }

        // The goal of this method is to perform the final matching:
        // Map between route values matched by the template and the ones we want to expose to the action for binding.
        // (tweaking the route values is fine here)
        // Invalidating the candidate if the key/function values are not valid/missing.
        // Perform overload resolution for functions by looking at the candidates and their metadata.
        for (var i = 0; i < candidates.Count; i++)
        {
            ref CandidateState candidate = ref candidates[i];
            if (!candidates.IsValidCandidate(i))
            {
                continue;
            }

            IODataRoutingMetadata? metadata = candidate.Endpoint.Metadata.OfType<IODataRoutingMetadata>().FirstOrDefault();
            if (metadata == null)
            {
                continue;
            }

            IEdmModel? model = GetEdmModel(candidate.Values);
            if (model == null)
            {
                continue;
            }

            try
            {
                ODataTemplateTranslateContext translatorContext = new(httpContext, candidate.Endpoint, candidate.Values, model);
                ODataPath odataPath = _translator.Translate(metadata.Template, translatorContext);
                if (odataPath != null)
                {
                    odataFeature.RoutePrefix = metadata.Prefix;
                    odataFeature.Model = model;
                    odataFeature.Path = odataPath;

                    MergeRouteValues(translatorContext.UpdatedValues, candidate.Values);
                }
                else
                {
                    candidates.SetValidity(i, false);
                }
            }
            catch
            {
            }
        }

        return Task.CompletedTask;
    }

    private static void MergeRouteValues(RouteValueDictionary updates, RouteValueDictionary? source)
    {
        if (source == null) return;
        foreach (var data in updates)
        {
            source[data.Key] = data.Value;
        }
    }

    private IEdmModel? GetEdmModel(RouteValueDictionary? routeValues)
    {
        if (routeValues == null)
        {
            return null;
        }

        if (!routeValues.TryGetValue("datasource", out object? datasourceObj))
        {
            return null;
        }

        string? dataSource = datasourceObj as string;
        if (dataSource == null)
        {
            return null;
        }


        return _dataSource.GetEdmModel(dataSource);
    }
}
