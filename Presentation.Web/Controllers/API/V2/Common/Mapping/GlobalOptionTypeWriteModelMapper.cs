using Core.ApplicationServices.Model.GlobalOptions;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public class GlobalOptionTypeWriteModelMapper : WriteModelMapperBase, IGlobalOptionTypeWriteModelMapper
{
    public GlobalOptionCreateParameters ToGlobalOptionCreateParameters(GlobalOptionCreateRequestDTO dto)
    {
        return new()
        {
            Description = dto.Description,
            IsObligatory = dto.IsObligatory,
            Name = dto.Name,
        };
    }

    public GlobalOptionTypeWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
    {
    }
}