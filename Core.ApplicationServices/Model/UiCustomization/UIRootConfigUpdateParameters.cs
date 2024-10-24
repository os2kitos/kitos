
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.UiCustomization
{
    public class UIRootConfigUpdateParameters
    {
        public Maybe<OptionalValueChange<bool>> ShowItContractModule { get; set; }
        public Maybe<OptionalValueChange<bool>> ShowDataProcessing { get; set; }
        public Maybe<OptionalValueChange<bool>> ShowItSystemModule { get; set; }
    }
}
