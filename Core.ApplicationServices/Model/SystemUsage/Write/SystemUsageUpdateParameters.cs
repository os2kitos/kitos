using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Model.SystemUsage.Write
{
    public class SystemUsageUpdateParameters
    {
        public Maybe<UpdatedSystemUsageGeneralProperties> GeneralProperties { get; set; } = Maybe<UpdatedSystemUsageGeneralProperties>.None;
    }
}
