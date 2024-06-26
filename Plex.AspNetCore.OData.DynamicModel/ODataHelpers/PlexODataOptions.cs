﻿namespace Plex.AspNetCore.OData.DynamicModel.ODataHelpers;
public class PlexODataOptions
{
    public string? ODataBaseAddress { get; set; }
    public string RoutePrefix { get; set; } = "odata";
    public string DynamicControllerName { get; set; } = "HandleAll";
    public int MaxPageSize { get; set; } = 1000;
    public int? MaxTopSize { get; set; }
    public string ContainerNamespaceName { get; set; } = "external";
    public string ContainerName { get; set; } = "container";
    public string[] ListSchemaNameSupport { get; set; } = { "external", "aggregated" };
    public string CustomerId { get; set; } = "customerid";
    public string LanguageId { get; set; } = "languageid";
    public string RefererToken { get; set; } = "referertoken";
}
