using Core.ApplicationServices.Model.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageUpdateParameters
    {
        public Maybe<ChangedValue<Maybe<UpdatedSystemUsageGeneralProperties>>> GeneralProperties { get; set; } = Maybe<ChangedValue<Maybe<UpdatedSystemUsageGeneralProperties>>>.None;
        //TODO: Add the other secions
    }
}
