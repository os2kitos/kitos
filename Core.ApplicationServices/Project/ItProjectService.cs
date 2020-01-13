using System;
using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;
using Core.DomainServices.Model.Result;
using Core.DomainServices.Repositories.Project;

namespace Core.ApplicationServices.Project
{
    public class ItProjectService : IItProjectService
    {
        private readonly IGenericRepository<ItProject> _projectRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItProjectRepository _itProjectRepository;

        public ItProjectService(
            IGenericRepository<ItProject> projectRepository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository itProjectRepository)
        {
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _itProjectRepository = itProjectRepository;
        }

        public TwoTrackResult<ItProject, OperationFailure> AddProject(ItProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (!_authorizationContext.AllowCreate<ItProject>(project))
            {
                return TwoTrackResult<ItProject, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            project.AccessModifier = AccessModifier.Local; //Force set to local
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

            return TwoTrackResult<ItProject, OperationFailure>.Success(project);
        }

        public TwoTrackResult<ItProject, OperationFailure> DeleteProject(int id)
        {
            var project = _projectRepository.GetByKey(id);
            if (project == null)
            {
                return TwoTrackResult<ItProject, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(project))
            {
                return TwoTrackResult<ItProject, OperationFailure>.Failure(OperationFailure.Forbidden);
            }
            _projectRepository.DeleteByKeyWithReferencePreload(id);
            _projectRepository.Save();

            return TwoTrackResult<ItProject, OperationFailure>.Success(project);
        }

        public IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null)
        {
            var projects = _itProjectRepository.GetProjects(
                new OrganizationDataQueryParameters(
                    organizationId,
                    OrganizationDataQueryBreadth.IncludePublicDataFromOtherOrganizations,
                    _authorizationContext.GetDataAccessLevel(organizationId)
                )
            );

            if (!string.IsNullOrWhiteSpace(optionalNameSearch))
            {
                projects = projects.ByPartOfName(optionalNameSearch);
            }

            return projects;
        }

        /// <summary>
        /// Adds default phases 1-5 and select the first phase as current phase
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private static void CreateDefaultPhases(ItProject project)
        {
            project.CurrentPhase = 1;
            project.Phase1 = new ItProjectPhase { Name = "Afventer" };
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
