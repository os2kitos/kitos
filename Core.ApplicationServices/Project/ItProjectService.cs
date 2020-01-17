using System;
using System.Collections.Generic;
using System.Data;
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
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Project
{
    public class ItProjectService : IItProjectService
    {
        private readonly IGenericRepository<ItProject> _projectRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItProjectRepository _itProjectRepository;
        private readonly ITransactionManager _transactionManager;

        public ItProjectService(
            IGenericRepository<ItProject> projectRepository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository itProjectRepository,
            ITransactionManager transactionManager)
        {
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _itProjectRepository = itProjectRepository;
            _transactionManager = transactionManager;
        }

        public Result<ItProject, OperationFailure> AddProject(ItProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (!_authorizationContext.AllowCreate<ItProject>(project))
            {
                return Result<ItProject, OperationFailure>.Failure(OperationFailure.Forbidden);
            }

            PrepareNewObject(project);

            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
            {
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
                transaction.Commit();
            }

            return Result<ItProject, OperationFailure>.Success(project);
        }

        private static void PrepareNewObject(ItProject project)
        {
            project.AccessModifier = AccessModifier.Local; //Force set to local

            //Setup all project phases and select initial "current".
            project.CurrentPhase = 1;
            project.Phase1 = new ItProjectPhase {Name = PhaseNames.Phase1};
            project.Phase2 = new ItProjectPhase {Name = PhaseNames.Phase2};
            project.Phase3 = new ItProjectPhase {Name = PhaseNames.Phase3};
            project.Phase4 = new ItProjectPhase {Name = PhaseNames.Phase4};
            project.Phase5 = new ItProjectPhase {Name = PhaseNames.Phase5};
        }

        public Result<ItProject, OperationFailure> DeleteProject(int id)
        {
            var project = _projectRepository.GetByKey(id);
            if (project == null)
            {
                return Result<ItProject, OperationFailure>.Failure(OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(project))
            {
                return Result<ItProject, OperationFailure>.Failure(OperationFailure.Forbidden);
            }
            _projectRepository.DeleteByKeyWithReferencePreload(id);
            _projectRepository.Save();

            return Result<ItProject, OperationFailure>.Success(project);
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
