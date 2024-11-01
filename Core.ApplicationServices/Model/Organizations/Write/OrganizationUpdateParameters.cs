using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Shared;

namespace Core.ApplicationServices.Model.Organizations.Write
{
    public class OrganizationUpdateParameters: OrganizationCvrUpdateParameter
    {
        public OptionalValueChange<Maybe<string>> Name { get; set; }

        public OptionalValueChange<Maybe<int>> TypeId { get; set; }

        public OptionalValueChange<Maybe<string>> ForeignCvr { get; set; }
    }
}
