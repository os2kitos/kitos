using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System.Regular;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class LegalPropertyWriteModelMapper : WriteModelMapperBase, ILegalPropertyWriteModelMapper
    {
        public LegalPropertyWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest) {}

        public LegalUpdateParameters FromPATCH(LegalPropertiesUpdateRequestDTO request)
        {
            var rule = CreateChangeRule<LegalPropertiesUpdateRequestDTO>(false);
            return new LegalUpdateParameters
            {
                SystemName = rule.MustUpdate(x => x.SystemName) ? request.SystemName.AsChangedValue() : OptionalValueChange<string>.None,
                DataProcessorName = rule.MustUpdate(x => x.DataProcessorName) ? request.DataProcessorName.AsChangedValue() : OptionalValueChange<string>.None
            };
        }
    }
}