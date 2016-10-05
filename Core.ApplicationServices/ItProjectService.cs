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

        public ItProject AddProject(ItProject project)
        {
            CreateDefaultPhases(project);
            _projectRepository.Insert(project);
            _projectRepository.Save();

            AddEconomyYears(project);

            project.Handover = new Handover()
                {
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                };

            project.GoalStatus = new GoalStatus()
                {
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                };

            _projectRepository.Save();

            return project;
        }

        // TODO cloning has been disabled for now, reviewed at a later date
        //public ItProject CloneProject(ItProject original, User newOwner, int newOrgId)
        //{
        //    var clone = _projectRepository.Create();
        //    clone.LastChangedByUser = newOwner;

        //    clone.OrganizationId = newOrgId;
        //    clone.ObjectOwner = newOwner;
        //    clone.OriginalId = original.Id;

        //    clone.ItProjectId = original.ItProjectId;
        //    clone.Background = original.Background;
        //    clone.IsTransversal = original.IsTransversal;
        //    clone.Name = original.Name;
        //    clone.Description = original.Description;
        //    clone.Description = original.Description;
        //    clone.IsStrategy = original.IsStrategy;

        //   // TODO clone actual data
        //    clone.Handover = new Handover()
        //        {
        //            ObjectOwner = newOwner,
        //            LastChangedByUser = newOwner
        //        };

        //    // TODO ParentId = project.ParentId,
        //    // TODO Children = project.Children,

        //    clone.ItProjectTypeId = original.ItProjectTypeId;
        //    clone.TaskRefs = original.TaskRefs;

        //    // TODO Risk
        //    // TODO Rights
        //    // TODO JointMunicipalProjectId = project.JointMunicipalProjectId,
        //    // TODO JointMunicipalProjects = project.JointMunicipalProjects,
        //    // TODO CommonPublicProjectId = project.CommonPublicProjectId,
        //    // TODO CommonPublicProjects = project.CommonPublicProjects,
        //    // TODO EconomyYears = project.EconomyYears
        //    // TODO MilestoneStates = project.MilestoneStates,
        //    // TODO TaskActivities = project.TaskActivities,
        //    // TODO Communcations = project.Communcations

        //        //TODO: clone this instead of creating new
        //    clone.GoalStatus = new GoalStatus()
        //        {
        //            ObjectOwner = newOwner,
        //            LastChangedByUser = newOwner
        //        };

        //    ClonePhases(original, clone);
        //    AddEconomyYears(clone);

        //    _projectRepository.Insert(clone);
        //    _projectRepository.Save();

        //    return clone;
        //}

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

        /// <summary>
        /// Clones the phases of a project to another project
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        /// <returns></returns>
        //private void ClonePhases(ItProject original, ItProject clone)
        //{
        //    var phase1 = CreatePhase(original.Phase1.Name, clone.ObjectOwner, clone);
        //    _phaseRepository.Insert(phase1);
        //    _phaseRepository.Save();

        //    clone.Phase1 = phase1;
        //    clone.CurrentPhase = phase1.Id;
        //    clone.Phase2 = CreatePhase(original.Phase2.Name, clone.ObjectOwner, clone);
        //    clone.Phase3 = CreatePhase(original.Phase3.Name, clone.ObjectOwner, clone);
        //    clone.Phase4 = CreatePhase(original.Phase4.Name, clone.ObjectOwner, clone);
        //    clone.Phase5 = CreatePhase(original.Phase5.Name, clone.ObjectOwner, clone);
        //}

        private static void AddEconomyYears(ItProject project)
        {
            project.EconomyYears = new List<EconomyYear>()
            {
                new EconomyYear()
                {
                    YearNumber = 0,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear()
                {
                    YearNumber = 1,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear()
                {
                    YearNumber = 2,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear()
                {
                    YearNumber = 3,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear()
                {
                    YearNumber = 4,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                },
                new EconomyYear()
                {
                    YearNumber = 5,
                    ObjectOwner = project.ObjectOwner,
                    LastChangedByUser = project.ObjectOwner
                }
            };
        }
    }
}
