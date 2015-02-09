using System;
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
        private readonly IGenericRepository<ItProjectPhase> _phaseRepository;

        public ItProjectService(IGenericRepository<ItProject> projectRepository, 
            IGenericRepository<ItProjectPhase> phaseRepository)
        {
            _projectRepository = projectRepository;
            _phaseRepository = phaseRepository;
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
            _projectRepository.Insert(project);
            _projectRepository.Save();
            
            CreateDefaultPhases(project);
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
        
        public ItProject CloneProject(ItProject original, User newOwner, int newOrgId)
        {
            var clone = _projectRepository.Create();
            clone.LastChangedByUser = newOwner;

            clone.OrganizationId = newOrgId;
            clone.ObjectOwner = newOwner;
            clone.OriginalId = original.Id;

            clone.ItProjectId = original.ItProjectId;
            clone.Background = original.Background;
            clone.IsTransversal = original.IsTransversal;
            clone.Name = original.Name;
            clone.Note = original.Note;
            clone.Description = original.Description;
            clone.IsStrategy = original.IsStrategy;

           // TODO clone actual data
            clone.Handover = new Handover()
                {
                    ObjectOwner = newOwner,
                    LastChangedByUser = newOwner
                };

            // TODO ParentId = project.ParentId,
            // TODO Children = project.Children,

            clone.ItProjectTypeId = original.ItProjectTypeId;
            clone.TaskRefs = original.TaskRefs;

            // TODO Risk
            // TODO Rights
            // TODO JointMunicipalProjectId = project.JointMunicipalProjectId,
            // TODO JointMunicipalProjects = project.JointMunicipalProjects,
            // TODO CommonPublicProjectId = project.CommonPublicProjectId,
            // TODO CommonPublicProjects = project.CommonPublicProjects,
            // TODO EconomyYears = project.EconomyYears
            // TODO MilestoneStates = project.MilestoneStates,
            // TODO TaskActivities = project.TaskActivities,
            // TODO Communcations = project.Communcations

                //TODO: clone this instead of creating new
            clone.GoalStatus = new GoalStatus()
                {
                    ObjectOwner = newOwner,
                    LastChangedByUser = newOwner
                };

            ClonePhases(original, clone);
            AddEconomyYears(clone);

            _projectRepository.Insert(clone);
            _projectRepository.Save();

            return clone;
        }

        public void DeleteProject(ItProject project)
        {
            //Remove reference to this project in cloned projects
            project.Clones.Select(clone => clone.Original = null); // TODO what's going on here?
            _projectRepository.Save();
            
            var phase1Id = project.Phase1.Id;
            var phase2Id = project.Phase2.Id;
            var phase3Id = project.Phase3.Id;
            var phase4Id = project.Phase4.Id;
            var phase5Id = project.Phase5.Id;
            
            _projectRepository.DeleteByKey(project.Id);
            _projectRepository.Save();

            //deleting phases - must be done afterwards, since they're required on project
            _phaseRepository.DeleteByKey(phase1Id);
            _phaseRepository.DeleteByKey(phase2Id);
            _phaseRepository.DeleteByKey(phase3Id);
            _phaseRepository.DeleteByKey(phase4Id);
            _phaseRepository.DeleteByKey(phase5Id);
            _phaseRepository.Save();
        }

        /// <summary>
        /// Adds default phases 1-5 and select the first phase as current phase 
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private void CreateDefaultPhases(ItProject project)
        {
            var phase1 = CreatePhase("Afventer", project.ObjectOwner, project);
            _phaseRepository.Insert(phase1);
            _phaseRepository.Save();

            project.Phase1 = phase1;
            project.CurrentPhaseId = phase1.Id;
            project.Phase2 = CreatePhase("Foranalyse", project.ObjectOwner, project);
            project.Phase3 = CreatePhase("Gennemførsel", project.ObjectOwner, project);
            project.Phase4 = CreatePhase("Overlevering", project.ObjectOwner, project);
            project.Phase5 = CreatePhase("Drift", project.ObjectOwner, project);

            _phaseRepository.Insert(project.Phase2);
            _phaseRepository.Insert(project.Phase3);
            _phaseRepository.Insert(project.Phase4);
            _phaseRepository.Insert(project.Phase5);
            _phaseRepository.Save();
        }

        /// <summary>
        /// Clones the phases of a project to another project
        /// </summary>
        /// <param name="original"></param>
        /// <param name="clone"></param>
        /// <returns></returns>
        private void ClonePhases(ItProject original, ItProject clone)
        {
            var phase1 = CreatePhase(original.Phase1.Name, clone.ObjectOwner, clone);
            _phaseRepository.Insert(phase1);
            _phaseRepository.Save();

            clone.Phase1 = phase1;
            clone.CurrentPhaseId = phase1.Id;
            clone.Phase2 = CreatePhase(original.Phase2.Name, clone.ObjectOwner, clone);
            clone.Phase3 = CreatePhase(original.Phase3.Name, clone.ObjectOwner, clone);
            clone.Phase4 = CreatePhase(original.Phase4.Name, clone.ObjectOwner, clone);
            clone.Phase5 = CreatePhase(original.Phase5.Name, clone.ObjectOwner, clone);
        }

        private ItProjectPhase CreatePhase(string name, User owner, ItProject project)
        {
            return new ItProjectPhase() { Name = name, ObjectOwner = owner, LastChangedByUser = owner, ItProject = project };
        }

        private void AddEconomyYears(ItProject project)
        {
            project.EconomyYears = new List<EconomyYear>()
                {
                    new EconomyYear()
                        {
                            YearNumber = 0, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        },
                    new EconomyYear()
                        {
                            YearNumber = 1, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        },
                    new EconomyYear()
                        {
                            YearNumber = 2, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        },
                    new EconomyYear()
                        {
                            YearNumber = 3, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        },
                    new EconomyYear()
                        {
                            YearNumber = 4, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        },
                    new EconomyYear()
                        {
                            YearNumber = 5, ObjectOwner = project.ObjectOwner, LastChangedByUser = project.ObjectOwner
                        }
                };
        }
    }
}