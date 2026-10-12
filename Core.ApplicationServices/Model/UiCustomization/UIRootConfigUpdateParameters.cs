
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.UiCustomization
{
    public class UIRootConfigUpdateParameters
    {
        public required OptionalValueChange<Maybe<bool>> ShowItContractModule { get; set; }
        public required OptionalValueChange<Maybe<bool>> ShowDataProcessing { get; set; }
        public required OptionalValueChange<Maybe<bool>> ShowItSystemModule { get; set; }
    }
}
