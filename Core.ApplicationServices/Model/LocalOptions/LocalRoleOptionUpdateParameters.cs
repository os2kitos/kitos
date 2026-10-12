using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.LocalOptions
{
    public class LocalRoleOptionUpdateParameters : LocalOptionUpdateParameters
    {
        public OptionalValueChange<Maybe<bool>> IsExternallyUsed { get; set; } = OptionalValueChange<Maybe<bool>>.None;

        public OptionalValueChange<Maybe<string>> ExternallyUsedDescription { get; set; } = OptionalValueChange<Maybe<string>>.None;
    }
}
