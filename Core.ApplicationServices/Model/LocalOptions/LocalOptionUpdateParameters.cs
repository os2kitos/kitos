using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.LocalOptions
{
    public class LocalOptionUpdateParameters
    {
        public OptionalValueChange<Maybe<string>> Description { get; set; }
    }
}
