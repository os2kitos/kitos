

using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.HelpTexts
{
    public class HelpTextUpdateParameters
    {
        public OptionalValueChange<Maybe<string>> Title { get; set; }
        public OptionalValueChange<Maybe<string>> Description { get; set; }
    }
}
