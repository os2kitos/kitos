using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.GlobalOptions
{
    public class GlobalRoleOptionUpdateParameters: GlobalRegularOptionUpdateParameters
    {
        public OptionalValueChange<Maybe<bool>> WriteAccess { get; set; }
    }
}
