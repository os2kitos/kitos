using Core.ApplicationServices.Model.Shared;
using Core.DomainModel.PublicMessage;

namespace Core.ApplicationServices.Model.Messages
{
    public class WritePublicMessagesParams
    {
        public OptionalValueChange<string> Title { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> LongDescription { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> ShortDescription { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<PublicMessageStatus?> Status { get; set; } = OptionalValueChange<PublicMessageStatus?>.None;
        public OptionalValueChange<string> Link { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<PublicMessageIconType?> IconType{ get; set; } = OptionalValueChange<PublicMessageIconType?>.None;
        public OptionalValueChange<bool> IsMain{ get; set; } = OptionalValueChange<bool>.None;
    }
}
