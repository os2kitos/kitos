using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Messages;
using Core.ApplicationServices.Model.Shared;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping
{
    public class PublicMessagesWriteModelMapper : WriteModelMapperBase, IPublicMessagesWriteModelMapper
    {
        public PublicMessagesWriteModelMapper(ICurrentHttpRequest currentHttpRequest) : base(currentHttpRequest)
        {
        }

        public WritePublicMessagesParams FromPATCH(PublicMessagesRequestDTO request)
        {
            var rule = CreateChangeRule<PublicMessagesRequestDTO>(false);

            return new WritePublicMessagesParams
            {
                Misc = rule.MustUpdate(x => x.Misc)
                    ? request.Misc.AsChangedValue()
                    : OptionalValueChange<string>.None,

                About = rule.MustUpdate(x => x.About)
                    ? request.About.AsChangedValue()
                    : OptionalValueChange<string>.None,

                ContactInfo = rule.MustUpdate(x => x.ContactInfo)
                    ? request.ContactInfo.AsChangedValue()
                    : OptionalValueChange<string>.None,

                StatusMessages = rule.MustUpdate(x => x.StatusMessages)
                    ? request.StatusMessages.AsChangedValue()
                    : OptionalValueChange<string>.None,

                Guides = rule.MustUpdate(x => x.Guides)
                    ? request.Guides.AsChangedValue()
                    : OptionalValueChange<string>.None
            };
        }
    }
}