using System.Collections.Generic;
using Core.Abstractions.Types;

namespace Infrastructure.STS.Common.Model
{
    public static class StsErrorParser
    {
        private static readonly IReadOnlyDictionary<string, StsError> KnownErrors = new Dictionary<string, StsError>
        {
            { "44", StsError.NotFound },
            { "40", StsError.BadInput }
        };
        public static Maybe<StsError> ParseStsError(this string resultCode)
        {
            if (resultCode == "20")
            {
                return Maybe<StsError>.None;
            }

            return KnownErrors.TryGetValue(resultCode, out var knownError) ? knownError : StsError.Unknown;
        }
    }
}
