using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class ItProjectService : IItProjectService
    {
        private readonly IGenericRepository<ItProject> _projectRepository;

        public ItProjectService(IGenericRepository<ItProject> projectRepository, IGenericRepository<ItProjectType> projectTypeRepository )
        {
            _projectRepository = projectRepository;
            ProgramType = projectTypeRepository.Get(type => type.Name == "IT Program").Single();
        }

        public ItProjectType ProgramType { get; private set; }

        public IEnumerable<ItProject> GetAll(Organization organization, string nameSearch)
        {
            if (nameSearch == null) return _projectRepository.Get();

            return _projectRepository.Get(project => project.Name.StartsWith(nameSearch));
        }

        public IEnumerable<ItProject> GetProjects(Organization organization, string nameSearch)
        {
            return GetAll(organization, nameSearch).Where(project => project.ItProjectType.Id != ProgramType.Id);
        }

        public IEnumerable<ItProject> GetPrograms(Organization organization, string nameSearch)
        {
            return GetAll(organization, nameSearch).Where(project => project.ItProjectType.Id == ProgramType.Id);
        }
    }
}