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
        private readonly IGenericRepository<Activity> _activityRepository;
        private readonly IGenericRepository<ItProjectRight> _rightRepository;
        private readonly IOrgService _orgService;

        public ItProjectService(IGenericRepository<ItProject> projectRepository, 
            IGenericRepository<ItProjectType> projectTypeRepository, 
            IGenericRepository<Activity> activityRepository, 
            IGenericRepository<ItProjectRight> rightRepository,
            IOrgService orgService)
        {
            _projectRepository = projectRepository;
            _activityRepository = activityRepository;
            _rightRepository = rightRepository;
            _orgService = orgService;

            //TODO: dont hardcode this
            ProgramType = projectTypeRepository.Get(type => type.Name == "IT Program").Single();
        }

        public ItProjectType ProgramType { get; private set; }

        public IEnumerable<ItProject> GetAll(int? orgId = null, string nameSearch = null, bool includePublic = true)
        {
            var result = _projectRepository.Get();

            if (orgId != null)
            {
                //filter by organisation or optionally by access modifier
                result =
                    result.Where(p => p.OrganizationId == orgId.Value || (includePublic && p.AccessModifier == AccessModifier.Public));
            }
            else
            {
                //if no organisation is selected, only get public
                result = result.Where(p => p.AccessModifier == AccessModifier.Public && includePublic);
            }


            //optionally filter by name
            if (nameSearch != null) result = result.Where(p => p.Name.Contains(nameSearch));

            return result;
        }

        public IEnumerable<ItProject> GetProjects(int? orgId = null, string nameSearch = null, bool includePublic = true)
        {
            return GetAll(orgId, nameSearch: nameSearch, includePublic: includePublic).Where(project => project.ItProjectType.Id != ProgramType.Id);
        }

        public IEnumerable<ItProject> GetPrograms(int? orgId = null, string nameSearch = null, bool includePublic = true)
        {
            return GetAll(orgId, nameSearch: nameSearch, includePublic: includePublic).Where(project => project.ItProjectType.Id == ProgramType.Id);
        }

        public ItProject AddProject(ItProject project)
        {
            CreateDefaultPhases(project);

            _projectRepository.Insert(project);
            _projectRepository.Save();

            return project;
        }
        
        public ItProject CloneProject(ItProject original, User newOwner, int newOrgId)
        {
            // TODO find a better approach, this is silly
            var clone = new ItProject()
            {
                OrganizationId = newOrgId,
                ObjectOwner = newOwner,
                ParentItProjectId = original.Id,

                ItProjectId = original.ItProjectId,
                Background = original.Background,
                IsTransversal = original.IsTransversal,
                Name = original.Name,
                Note = original.Note,
                Description = original.Description,
                AccessModifier = AccessModifier.Normal,
                IsStrategy = original.IsStrategy,

                // TODO AssociatedProgramId = project.AssociatedProgramId,
                // TODO AssociatedProjects = project.AssociatedProjects,
                ItProjectTypeId = original.ItProjectTypeId,
                ItProjectCategoryId = original.ItProjectCategoryId,
                TaskRefs = original.TaskRefs,
                // TODO Risk
                // TODO Rights
                // TODO JointMunicipalProjectId = project.JointMunicipalProjectId,
                // TODO JointMunicipalProjects = project.JointMunicipalProjects,
                // TODO CommonPublicProjectId = project.CommonPublicProjectId,
                // TODO CommonPublicProjects = project.CommonPublicProjects,
                // TODO EconomyYears = project.EconomyYears
                // TODO MilestoneStates = project.MilestoneStates,
                // TODO TaskActivities = project.TaskActivities
            };

            ClonePhases(original, clone);

            _projectRepository.Insert(clone);
            _projectRepository.Save();

            return clone;
        }

        public void DeleteProject(ItProject project)
        {
            var phase1Id = project.Phase1.Id;
            var phase2Id = project.Phase1.Id;
            var phase3Id = project.Phase1.Id;
            var phase4Id = project.Phase1.Id;
            var phase5Id = project.Phase1.Id;

            _projectRepository.DeleteByKey(project.Id);
            _projectRepository.Save();

            _activityRepository.DeleteByKey(phase1Id);
            _activityRepository.DeleteByKey(phase2Id);
            _activityRepository.DeleteByKey(phase3Id);
            _activityRepository.DeleteByKey(phase4Id);
            _activityRepository.DeleteByKey(phase5Id);
            _activityRepository.Save();

        }

        public bool HasReadAccess(User user, ItProject project)
        {
            //if the project is public, the user has read access
            if (project.AccessModifier == AccessModifier.Public) return true;

            //if the user is object owner, the user has read access
            if (project.ObjectOwnerId == user.Id) return true;

            //if the user has read role, the user has read access
            if (_rightRepository.Get(right => 
                right.User.Id == user.Id &&
                right.Object.Id == project.Id && 
                right.Role.HasReadAccess).Any()) 
                return true;

            //if the user is part of an organization, which owns the project, the user has read access
            var userOrganizations = _orgService.GetByUser(user);
            return userOrganizations.Any(org => org.Id == project.OrganizationId);
        }

        public bool HasWriteAccess(User user, ItProject project)
        {
            if (project.ObjectOwnerId == user.Id) return true;

            return
                _rightRepository.Get(right => right.User.Id == user.Id && right.Object.Id == project.Id && right.Role.HasWriteAccess)
                                .Any();
        }

        /// <summary>
        /// Adds default phases 1-5 and select the first phase as current phase 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private void CreateDefaultPhases(ItProject project)
        {

            //TODO: use global config names of default phases
            var phase1 = CreatePhase("Afventer", project.ObjectOwner);
            _activityRepository.Insert(phase1);
            _activityRepository.Save();

            project.Phase1 = phase1;
            project.CurrentPhaseId = phase1.Id;
            project.Phase2 = CreatePhase("Foranalyse", project.ObjectOwner);
            project.Phase3 = CreatePhase("Gennemførsel", project.ObjectOwner);
            project.Phase4 = CreatePhase("Overlevering", project.ObjectOwner);
            project.Phase5 = CreatePhase("Drif", project.ObjectOwner);
        }

        /// <summary>
        /// Clones the phases of a project to another project
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        /// <returns></returns>
        private void ClonePhases(ItProject original, ItProject clone)
        {
            var phase1 = CreatePhase(original.Phase1.Name, clone.ObjectOwner);
            _activityRepository.Insert(phase1);
            _activityRepository.Save();

            clone.Phase1 = phase1;
            clone.CurrentPhaseId = phase1.Id;
            clone.Phase2 = CreatePhase(original.Phase2.Name, clone.ObjectOwner);
            clone.Phase3 = CreatePhase(original.Phase3.Name, clone.ObjectOwner);
            clone.Phase4 = CreatePhase(original.Phase4.Name, clone.ObjectOwner);
            clone.Phase5 = CreatePhase(original.Phase5.Name, clone.ObjectOwner);
        }

        private Activity CreatePhase(string name, User owner)
        {
            return new Activity() {Name = name, ObjectOwner = owner};
        }
    }
}