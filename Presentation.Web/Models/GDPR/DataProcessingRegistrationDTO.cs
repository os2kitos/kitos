using Core.DomainModel.Shared;
using Presentation.Web.Models.References;
using Presentation.Web.Models;

namespace Presentation.Web.Models.GDPR
{
    public class DataProcessingRegistrationDTO : NamedEntityDTO
    {
        public DataProcessingRegistrationDTO(int id, string name)
            : base(id, name)
        {
        }

        public AssignedRoleDTO[] AssignedRoles { get; set; }

        public ReferenceDTO[] References { get; set; }

        public NamedEntityWithEnabledStatusDTO[] ItSystems { get; set; }

        public ValueOptionWithOptionalNoteDTO<YearMonthIntervalOption?> OversightInterval { get; set; }

        public ShallowOrganizationDTO[] DataProcessors { get; set; }
    }
}