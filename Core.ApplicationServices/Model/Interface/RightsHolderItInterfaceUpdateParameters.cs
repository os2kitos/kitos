using Core.ApplicationServices.Model.Shared;
using System;

namespace Core.ApplicationServices.Model.Interface
{
    public class RightsHolderItInterfaceUpdateParameters
    {
        public OptionalValueChange<Guid> ExposingSystemUuid { get; set; } = OptionalValueChange<Guid>.None;
        public OptionalValueChange<string> Name { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> InterfaceId { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Version { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> Description { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> UrlReference { get; set; } = OptionalValueChange<string>.None;
    }
}
