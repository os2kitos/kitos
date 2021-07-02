using System;
using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Extensions;
using Core.DomainServices.Factories;
using Core.DomainServices.Model;
using Core.DomainServices.Queries;
using Core.DomainServices.Repositories.Project;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Types;

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
        private readonly IOrganizationalUserContext _userContext;

        public ItProjectService(
            IGenericRepository<ItProject> projectRepository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository itProjectRepository,
            IUserRepository userRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IOrganizationalUserContext userContext)
        {
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _itProjectRepository = itProjectRepository;
            _userRepository = userRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _userContext = userContext;
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

            _projectRepository.Insert(project);
            _projectRepository.Save();
            _domainEvents.Raise(new EntityCreatedEvent<ItProject>(project));
            return project;
        }

        public Result<ItProject, OperationFailure> DeleteProject(int id)
        {
            using (var transaction = _transactionManager.Begin(IsolationLevel.ReadCommitted))
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
                _domainEvents.Raise(new EntityDeletedEvent<ItProject>(project));
                _projectRepository.DeleteWithReferencePreload(project);
                _projectRepository.Save();
                transaction.Commit();
                return project;
            }
        }

        public IQueryable<ItProject> GetAvailableProjects(params IDomainQuery<ItProject>[] conditions)
        {
            var userOrganizationsIds = _userContext.OrganizationIds;
            var refinement = new QueryByOrganizationIds<ItProject>(userOrganizationsIds);
            var mainQuery = _itProjectRepository.GetProjects();
            var refinedResult = refinement.Apply(mainQuery);

            return conditions.Any() ? new IntersectionQuery<ItProject>(conditions).Apply(refinedResult) : refinedResult;
        }

        public IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null)
        {
            var projects = _itProjectRepository.GetProjects(organizationId);

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
                    project => _authorizationContext.AllowReads(project) ? Result<ItProject, OperationError>.Success(project) : new OperationError(OperationFailure.Forbidden),
                    () => new OperationError(OperationFailure.NotFound)
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
