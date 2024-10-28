
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.UiCustomization
{
    public class UIRootConfigUpdateParameters
    {
        public OptionalValueChange<Maybe<bool>> ShowItContractModule { get; set; }
        public OptionalValueChange<Maybe<bool>> ShowDataProcessing { get; set; }
        public OptionalValueChange<Maybe<bool>> ShowItSystemModule { get; set; }
    }
}
