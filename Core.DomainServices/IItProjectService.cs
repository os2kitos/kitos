using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace Core.DomainServices
{
    public interface IItProjectService
    {
        ItProjectType ProgramType { get; }

        IEnumerable<ItProject> GetAll(Organization organization, string nameSearch);
        IEnumerable<ItProject> GetProjects(Organization organization, string nameSearch);
        IEnumerable<ItProject> GetPrograms(Organization organization, string nameSearch);
    }
}