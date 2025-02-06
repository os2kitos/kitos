using Presentation.Web.Models.API.V2.Request.User;
using System;

namespace Presentation.Web.Controllers.API.V2.Internal.Users.Mapping
{
    public class DefaultUserStartPreferenceChoiceMapper
    {
        public static string GetDefaultUserStartPreferenceString(DefaultUserStartPreferenceChoice preference)
        {
            switch (preference)
            {
                case DefaultUserStartPreferenceChoice.StartSite:
                    return "index";
                case DefaultUserStartPreferenceChoice.DataProcessing:
                    return "data-processing.overview";
                case DefaultUserStartPreferenceChoice.ItContract:
                    return "it-contract.overview";
                case DefaultUserStartPreferenceChoice.ItSystemCatalog:
                    return "it-system.catalog";
                case DefaultUserStartPreferenceChoice.ItSystemUsage:
                    return "it-system.overview";
                case DefaultUserStartPreferenceChoice.Organization:
                    return "organization.structure";
                default:
                    throw new ArgumentException($"GetDefaultUserStartPreference for type {preference} not implemented");
            }
        }

        public static DefaultUserStartPreferenceChoice GetDefaultUserStartPreferenceChoice(string preference)
        {
            switch (preference)
            {
                case null:
                case "index":
                    return DefaultUserStartPreferenceChoice.StartSite;
                case "data-processing.overview":
                    return DefaultUserStartPreferenceChoice.DataProcessing;
                case "it-contract.overview":
                    return DefaultUserStartPreferenceChoice.ItContract;
                case "it-system.catalog":
                    return DefaultUserStartPreferenceChoice.ItSystemCatalog;
                case "it-system.overview":
                    return DefaultUserStartPreferenceChoice.ItSystemUsage;
                case "organization.structure":
                    return DefaultUserStartPreferenceChoice.Organization;
                default:
                    throw new ArgumentException($"GetDefaultUserStartPreference for type {preference} not implemented");
            }
        }
    }
}