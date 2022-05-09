using System;
using System.Data;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Factories;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Project;
using Infrastructure.Services.DataAccess;


namespace Core.ApplicationServices.Project
{
    public class ItProjectService : IItProjectService
    {
        private readonly IGenericRepository<ItProject> _projectRepository;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItProjectRepository _itProjectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReferenceService _referenceService;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IOrganizationService _organizationService;

        public ItProjectService(
            IGenericRepository<ItProject> projectRepository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository itProjectRepository,
            IUserRepository userRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IOrganizationService organizationService)
        {
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _itProjectRepository = itProjectRepository;
            _userRepository = userRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _organizationService = organizationService;
        }

        public Result<ItProject, OperationFailure> AddProject(string name, int organizationId)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name.Length > ItProjectConstraints.MaxNameLength)
            {
                return OperationFailure.BadInput;
            }
            var project = ItProjectFactory.Create(name, organizationId);

            if (!_authorizationContext.AllowCreate<ItProject>(organizationId, project))
            {
                return OperationFailure.Forbidden;
            }

            var canCreateNewProjectWithName = CanCreateNewProjectWithName(name, organizationId);
            if (canCreateNewProjectWithName.Failed)
                return canCreateNewProjectWithName.Error.FailureType;

            if (canCreateNewProjectWithName.Value == false)
                return OperationFailure.Conflict;

            _projectRepository.Insert(project);
            _projectRepository.Save();
            _domainEvents.Raise(new EntityCreatedEvent<ItProject>(project));
            return project;
        }

        public Result<ItProject, OperationFailure> DeleteProject(int id)
        {
            using (var transaction = _transactionManager.Begin())
            {
                var project = _projectRepository.GetByKey(id);
                if (project == null)
                {
                    return OperationFailure.NotFound;
                }

                if (!_authorizationContext.AllowDelete(project))
                {
                    return OperationFailure.Forbidden;
                }

                var deleteByProjectId = _referenceService.DeleteByProjectId(id);
                if (deleteByProjectId.Failed)
                {
                    transaction.Rollback();
                    return deleteByProjectId.Error;
                }
                project.Handover?.Participants?.Clear();
                _domainEvents.Raise(new EntityBeingDeletedEvent<ItProject>(project));
                _projectRepository.DeleteWithReferencePreload(project);
                _projectRepository.Save();
                transaction.Commit();
                return project;
            }
        }

        public Result<IQueryable<ItProject>, OperationError> GetProjectsInOrganization(Guid organizationUuid, params IDomainQuery<ItProject>[] conditions)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind(organization =>
                {
                    var query = new IntersectionQuery<ItProject>(conditions);

                    return _itProjectRepository
                        .GetProjectsInOrganization(organization.Id)
                        .Transform(query.Apply)
                        .Transform(Result<IQueryable<ItProject>, OperationError>.Success);
                });
        }

        public IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null)
        {
            var projects = _itProjectRepository.GetProjectsInOrganization(organizationId);

            if (!string.IsNullOrWhiteSpace(optionalNameSearch))
            {
                projects = projects.ByPartOfName(optionalNameSearch);
            }

            return projects;
        }

        public Result<Handover, OperationFailure> AddHandoverParticipant(int projectId, int participantId)
        {
            var itProject = _itProjectRepository.GetById(projectId);
            var user = _userRepository.GetById(participantId);
            var error = CanAddParticipant(itProject, user);
            if (error.HasValue)
            {
                return error.Value;
            }

            itProject.Handover.Participants.Add(user);
            _projectRepository.Save();

            return itProject.Handover;
        }

        private Maybe<OperationFailure> CanAddParticipant(ItProject itProject, User user)
        {
            if (itProject == null)
            {
                return OperationFailure.NotFound;
            }
            if (!_authorizationContext.AllowModify(itProject))
            {
                return OperationFailure.Forbidden;
            }
            if (user == null)
            {
                return OperationFailure.BadInput;
            }
            if (itProject.Handover.Participants.Any(p => p.Id == user.Id))
            {
                return OperationFailure.Conflict;
            }
            return Maybe<OperationFailure>.None;
        }

        public Result<Handover, OperationFailure> DeleteHandoverParticipant(int projectId, int participantId)
        {
            var itProject = _itProjectRepository.GetById(projectId);
            var user = _userRepository.GetById(participantId);
            var error = CanRemoveParticipant(itProject, user);

            if (error.HasValue)
            {
                return error.Value;
            }

            itProject.Handover.Participants.Remove(user);
            _projectRepository.Save();

            return itProject.Handover;
        }

        public Result<ItProject, OperationError> GetProject(Guid uuid)
        {
            return _itProjectRepository
                .GetProject(uuid)
                .Match
                (
                    WithReadAccess,
                    () => new OperationError(OperationFailure.NotFound)
                );
        }

        private Result<ItProject, OperationError> WithReadAccess(ItProject project)
        {
            return _authorizationContext.AllowReads(project) ? Result<ItProject, OperationError>.Success(project) : new OperationError(OperationFailure.Forbidden);
        }

        public Result<bool, OperationError> CanCreateNewProjectWithName(string name, int orgId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (_authorizationContext.GetOrganizationReadAccessLevel(orgId) < OrganizationDataReadAccessLevel.All)
                return new OperationError(OperationFailure.Forbidden);

            return _itProjectRepository.GetProjectsInOrganization(orgId).ByNameExact(name).Any() == false;
        }

        public Maybe<OperationError> ValidateNewName(int projectId, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return new OperationError(OperationFailure.BadInput);

            return _itProjectRepository
                .GetById(projectId)
                .FromNullable()
                .Match(WithReadAccess, () => new OperationError(OperationFailure.NotFound))
                .Select(project => _itProjectRepository.GetProjectsInOrganization(project.OrganizationId).ByNameExact(newName).ExceptEntitiesWithIds(projectId).Any())
                .Match
                (
                    overlapsFound =>
                        overlapsFound ?
                            new OperationError(OperationFailure.Conflict) :
                            Maybe<OperationError>.None,
                    error => error
                );
        }

        private Maybe<OperationFailure> CanRemoveParticipant(ItProject itProject, User user)
        {
            if (itProject == null)
            {
                return OperationFailure.NotFound;
            }

            if (!_authorizationContext.AllowModify(itProject))
            {
                return OperationFailure.Forbidden;
            }

            if (user == null)
            {
                return OperationFailure.BadInput;
            }

            if (itProject.Handover.Participants.Any(p => p.Id == user.Id) == false)
            {
                return OperationFailure.BadInput;
            }
            return Maybe<OperationFailure>.None;
        }

    }
}
