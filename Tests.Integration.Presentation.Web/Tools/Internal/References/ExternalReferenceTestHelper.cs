using System;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Integration.Presentation.Web.Tools.Internal.References
{
    public class ExternalReferenceTestHelper
    {
        public static IEnumerable<T> WithRandomMaster<T>(IEnumerable<T> references) where T : ExternalReferenceDataWriteRequestDTO
        {
            var orderedRandomly = references.OrderBy(_ => Guid.NewGuid()).ToList();
            orderedRandomly.First().MasterReference = true;
            foreach (var externalReferenceDataDto in orderedRandomly.Skip(1))
                externalReferenceDataDto.MasterReference = false;

            return orderedRandomly;
        }
    }
}
