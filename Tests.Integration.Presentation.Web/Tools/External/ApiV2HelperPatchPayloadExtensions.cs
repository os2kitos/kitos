using System.Collections.Generic;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ApiV2HelperPatchPayloadExtensions
    {
        public static Dictionary<string, object> AsPatchPayloadOfProperty(this object dto, string propertyName)
        {
            var payload = new Dictionary<string, object>
            {
                {propertyName,dto}
            };
            return payload;
        }
    }
}
