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

        public ItProjectService(IGenericRepository<ItProject> projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public IEnumerable<ItProject> GetAll(int? orgId = null, string nameSearch = null, bool includePublic = true)
        {
            var result = _projectRepository.Get();

            if (orgId != null)
            {
                //filter by organisation or optionally by access modifier
                result = result.Where(p => p.OrganizationId == orgId.Value || (includePublic && p.AccessModifier == AccessModifier.Public));
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

        public ItProject AddProject(ItProject project)
        {
            CreateDefaultPhases(project);
            _projectRepository.Insert(project);
            _projectRepository.Save();

            AddEconomyYears(project);

            project.Handover = new Handover
                {
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                };

            project.GoalStatus = new GoalStatus
                {
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                };

            _projectRepository.Save();

            return project;
        }

        public void DeleteProject(int id)
        {
            // http://stackoverflow.com/questions/15226312/entityframewok-how-to-configure-cascade-delete-to-nullify-foreign-keys
            // when children are loaded into memory the foreign key is correctly set to null on children when deleted
            var project = _projectRepository.Get(x => x.Id == id, null, $"{nameof(ItProject.Children)}, {nameof(ItProject.JointMunicipalProjects)}, {nameof(ItProject.CommonPublicProjects)}, {nameof(ItProject.TaskRefs)}, {nameof(ItProject.ItSystemUsages)}").FirstOrDefault();

            // delete it project
            _projectRepository.Delete(project);
            _projectRepository.Save();
        }

        /// <summary>
        /// Adds default phases 1-5 and select the first phase as current phase
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static void CreateDefaultPhases(ItProject project)
        {
            project.CurrentPhase = 1;
            project.Phase1 = new ItProjectPhase {Name = "Afventer"};
            project.Phase2 = new ItProjectPhase { Name = "Foranalyse" };
            project.Phase3 = new ItProjectPhase { Name = "Gennemførsel" };
            project.Phase4 = new ItProjectPhase { Name = "Overlevering" };
            project.Phase5 = new ItProjectPhase { Name = "Drift" };
        }

       private static void AddEconomyYears(ItProject project)
        {
            project.EconomyYears = new List<EconomyYear>
            {
                new EconomyYear
                {
                    YearNumber = 0,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear
                {
                    YearNumber = 1,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear
                {
                    YearNumber = 2,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear
                {
                    YearNumber = 3,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear
                {
                    YearNumber = 4,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear
                {
                    YearNumber = 5,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                }
            };
        }
    }
}
