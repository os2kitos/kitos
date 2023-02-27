using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http.Formatting;

namespace Presentation.Web.Infrastructure.Config
{
    public static class V2JsonSerializationConfig
    {
        private static JsonMediaTypeFormatter _jsonMediaTypeFormatter = null;

        public static JsonMediaTypeFormatter JsonMediaTypeFormatter
        {
            get
            {
                if (_jsonMediaTypeFormatter == null)
                {
                    throw new InvalidOperationException("V2 json formatter has not been configured yet");
                }

                return _jsonMediaTypeFormatter;
            }
        }

        public static void Configure(JsonMediaTypeFormatter referenceJsonFormatter)
        {
            var serializerSettings = new JsonSerializerSettings(referenceJsonFormatter.SerializerSettings);
            serializerSettings.Converters.Add(new StringEnumConverter());
            var updatedMediaTypeFormatter = new JsonMediaTypeFormatter
            {
                Indent = referenceJsonFormatter.Indent,
                MaxDepth = referenceJsonFormatter.MaxDepth,
                RequiredMemberSelector = referenceJsonFormatter.RequiredMemberSelector,
                UseDataContractJsonSerializer = referenceJsonFormatter.UseDataContractJsonSerializer,
                SerializerSettings = serializerSettings
            };
            referenceJsonFormatter
                .MediaTypeMappings
                .Except(updatedMediaTypeFormatter.MediaTypeMappings).ToList()
                .ForEach(updatedMediaTypeFormatter.MediaTypeMappings.Add);

            referenceJsonFormatter
                .SupportedEncodings
                .Except(updatedMediaTypeFormatter.SupportedEncodings).ToList()
                .ForEach(updatedMediaTypeFormatter.SupportedEncodings.Add);

            referenceJsonFormatter
                .SupportedMediaTypes
                .Except(updatedMediaTypeFormatter.SupportedMediaTypes).ToList()
                .ForEach(updatedMediaTypeFormatter.SupportedMediaTypes.Add);

            _jsonMediaTypeFormatter = updatedMediaTypeFormatter;
        }
    }
}