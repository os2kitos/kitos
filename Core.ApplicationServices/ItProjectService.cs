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

        public ItProjectService(IGenericRepository<ItProject> projectRepository, IGenericRepository<ItProjectType> projectTypeRepository, IGenericRepository<Activity> activityRepository )
        {
            _projectRepository = projectRepository;
            _activityRepository = activityRepository;

            //TODO: dont hardcode this
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

        public ItProject AddProject(ItProject project)
        {
            CreateDefaultPhases(project);
            AddEconomyYears(project);

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
                AccessModifier = original.AccessModifier,
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
            AddEconomyYears(clone);

            _projectRepository.Insert(clone);
            _projectRepository.Save();

            return clone;
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

        private void AddEconomyYears(ItProject project)
        {
            project.EconomyYears = new List<EconomyYear>()
                {
                    new EconomyYear()
                        {
                            YearNumber = 0
                        },
                    new EconomyYear()
                        {
                            YearNumber = 1
                        },
                    new EconomyYear()
                        {
                            YearNumber = 2
                        },
                    new EconomyYear()
                        {
                            YearNumber = 3
                        },
                    new EconomyYear()
                        {
                            YearNumber = 4
                        },
                    new EconomyYear()
                        {
                            YearNumber = 5
                        }
                };
        }
    }
}