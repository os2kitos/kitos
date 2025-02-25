using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.System
{
    public class DBSUpdateParameters
    {
        public OptionalValueChange<string> SystemName { get; set; } = OptionalValueChange<string>.None;
        public OptionalValueChange<string> DataProcessorName { get; set; } = OptionalValueChange<string>.None;
    }
}
