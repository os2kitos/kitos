using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Interface
{
    public class ItInterfaceWriteModel : ItInterfaceWriteModelParametersBase
    {
        public OptionalValueChange<bool> Deactivated { get; set; } = OptionalValueChange<bool>.None;
    }
}
