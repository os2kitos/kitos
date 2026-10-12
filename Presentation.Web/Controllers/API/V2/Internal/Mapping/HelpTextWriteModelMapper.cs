using Core.Abstractions.Extensions;
using Core.ApplicationServices.Model.HelpTexts;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Internal.Mapping;

public class HelpTextWriteModelMapper : WriteModelMapperBase, IHelpTextWriteModelMapper
{
    public HelpTextCreateParameters ToCreateParameters(HelpTextCreateRequestDTO dto)
    {
        return new()
        {
            Key = dto.Key ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Title = dto.Title ?? string.Empty
        };
    }

    public HelpTextUpdateParameters ToUpdateParameters(HelpTextUpdateRequestDTO dto)
    {
        var rule = CreateChangeRule<HelpTextUpdateRequestDTO>(false);

        return new()
        {
            Title = GetOptionalValueChange(() => rule.MustUpdate(x => x.Title), dto.Title.FromNullable()),
            Description = GetOptionalValueChange(() => rule.MustUpdate(x => x.Description), dto.Description.FromNullable())
        };
    }

    public HelpTextWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
    {
    }
}