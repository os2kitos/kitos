
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.GlobalOptions
{
    public class GlobalRegularOptionUpdateParameters
    {
        public OptionalValueChange<Maybe<bool>> IsEnabled { get; set; }
        public OptionalValueChange<Maybe<string>> Name { get; set; }
        public OptionalValueChange<Maybe<bool>> IsObligatory { get; set; }
        public OptionalValueChange<Maybe<string>> Description { get; set; }
        public OptionalValueChange<Maybe<int>> Priority { get; set; }
    }
}
