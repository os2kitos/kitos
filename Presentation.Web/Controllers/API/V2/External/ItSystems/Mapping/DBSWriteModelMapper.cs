using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.System;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Request.System.Regular;

namespace Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping
{
    public class DBSWriteModelMapper : WriteModelMapperBase, IDBSWriteModelMapper
    {
        public DBSWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest) {}

        public DBSUpdateParameters FromPATCH(UpdateDBSPropertiesRequestDTO request)
        {
            var rule = CreateChangeRule<UpdateDBSPropertiesRequestDTO>(false);
            return new DBSUpdateParameters
            {
                SystemName = rule.MustUpdate(x => x.SystemName) ? request.SystemName.AsChangedValue() : OptionalValueChange<string>.None,
                DataProcessorName = rule.MustUpdate(x => x.DataProcessorName) ? request.DataProcessorName.AsChangedValue() : OptionalValueChange<string>.None
            };
        }
    }
}