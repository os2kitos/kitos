using System;
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
        public static Maybe<StsError> ParseStsErrorFromStandardResultCode(this string resultCode)
        {
            if (resultCode == "20")
            {
                return Maybe<StsError>.None;
            }

            return KnownErrors.TryGetValue(resultCode, out var knownError) ? knownError : StsError.Unknown;
        }

        public static Maybe<StsError> ParseStsFromErrorCode(this string errorCode)
        {
            if (errorCode != null)
            {
                if (errorCode.Equals("ServiceAgreementNotFound", StringComparison.OrdinalIgnoreCase))
                {
                    return StsError.MissingServiceAgreement;
                }
                if (errorCode.IndexOf("Usercontext doesn't exist", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return StsError.ReceivedUserContextDoesNotExistOnSystem;
                }
                if (errorCode.Contains("ServiceAgreement"))
                {
                    //Covers a lot of different erros related to the service agreement: https://www.serviceplatformen.dk/administration/errorcodes-doc/errorcodes/4afb35be-7b7a-45b3-ab01-bd5017a8b182_errorcodes.html
                    return StsError.ExistingServiceAgreementIssue;
                }
            }

            return StsError.Unknown;
        }
    }
}
