using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Messages
{
    public class WritePublicMessagesParams
    {
        public OptionalValueChange<string> About { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Guides { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> StatusMessages { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Misc { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> ContactInfo { get; set; } = OptionalValueChange<string>.None;
    }
}
