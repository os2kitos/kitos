using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Messages;
using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.PublicMessage;
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

        public WritePublicMessagesParams FromPOST(PublicMessageRequestDTO request)
        {
            return MapParams(request, true);
        }

        public WritePublicMessagesParams FromPATCH(PublicMessageRequestDTO request)
        {
            return MapParams(request, false);
        }

        private WritePublicMessagesParams MapParams(PublicMessageRequestDTO request, bool enforceChanges)
        {
            var rule = CreateChangeRule<PublicMessageRequestDTO>(enforceChanges);

            return new WritePublicMessagesParams
            {
                Title = rule.MustUpdate(x => x.Title)
                    ? request.Title.AsChangedValue()
                    : OptionalValueChange<string>.None,
                LongDescription = rule.MustUpdate(x => x.LongDescription)
                    ? request.LongDescription.AsChangedValue()
                    : OptionalValueChange<string>.None,
                Link = rule.MustUpdate(x => x.Link)
                    ? request.Link.AsChangedValue()
                    : OptionalValueChange<string>.None,
                ShortDescription = rule.MustUpdate(x => x.ShortDescription)
                    ? request.ShortDescription.AsChangedValue()
                    : OptionalValueChange<string>.None,
                Status = rule.MustUpdate(x => x.Status)
                    ? (request.Status?.ToPublicMessageStatus()).AsChangedValue()
                    : OptionalValueChange<PublicMessageStatus?>.None
            };

        }
    }
}