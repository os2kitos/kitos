using System.Collections.Generic;
using System.Linq;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ApiV2HelperPatchPayloadExtensions
    {
        public static Dictionary<string, object> AsPatchPayloadOfProperty(this object dto, string propertyName)
        {
            var payload = new Dictionary<string, object>
            {
                {propertyName, dto}
            };
            return payload;
        }

        public static Dictionary<string, object> AsPatchPayloadOfMultipleProperties(this object dto,
            params string[] propertyNames)
        {
            var reversedProperties = propertyNames.Reverse().ToList();
            var firstProperty = reversedProperties.First();
            reversedProperties.Remove(firstProperty);

            var payload = dto.AsPatchPayloadOfProperty(firstProperty);
            return reversedProperties.Aggregate(payload, (current, propertyName) => current.AsPatchPayloadOfProperty(propertyName));
        }
    }
}
