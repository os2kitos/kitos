using System.Data;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model;
using Core.ApplicationServices.References;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Factories;
using Core.DomainServices.Model;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Time;
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

        public ItProjectService(
            IGenericRepository<ItProject> projectRepository,
            IAuthorizationContext authorizationContext,
            IItProjectRepository itProjectRepository,
            IUserRepository userRepository,
            IReferenceService referenceService,
            ITransactionManager transactionManager)
        {
            _projectRepository = projectRepository;
            _authorizationContext = authorizationContext;
            _itProjectRepository = itProjectRepository;
            _userRepository = userRepository;
            _referenceService = referenceService;
            _transactionManager = transactionManager;
        }

        public Result<ItProject, OperationFailure> AddProject(string name, int organizationId)
        {
            if (name.Length > ItProjectConstraints.MaxNameLength)
            {
                return OperationFailure.BadInput;
            }
            var project = ItProjectFactory.Create(name, organizationId);

            if (!_authorizationContext.AllowCreate<ItProject>(project))
            {
                return OperationFailure.Forbidden;
            }

            _projectRepository.Insert(project);
            _projectRepository.Save();
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
                _projectRepository.DeleteWithReferencePreload(project);
                _projectRepository.Save();
                transaction.Commit();
                return project;
            }
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
