using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Model.Authentication;

namespace Presentation.Web.Access
{
    public class OrganizationAccessContext
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IFeatureChecker _featureChecker;
        private readonly IAuthenticationContext _authenticationContext;
        private readonly IGenericRepository<ItSystemRole> _systemRoleRepository;
        private readonly int _organizationId;

        public OrganizationAccessContext(
            IGenericRepository<User> userRepository,
            IFeatureChecker featureChecker,
            IAuthenticationContext authenticationContext,
            int organizationId)
        {
            _userRepository = userRepository;
            _featureChecker = featureChecker;
            _authenticationContext = authenticationContext;
            _organizationId = organizationId;
        }

        public bool AllowReads(int userId)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (user.IsGlobalAdmin)
            {
                result = true;
            }
            else if (TargetOrganizationMatchesActiveOrganization())
            {
                result = true;
            }

            if (IsUserInMunicipality(user))
            {
                result = true;
            }

            return result;
        }

        public bool AllowReads(int userId, IEntity entity)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (user.IsGlobalAdmin)
            {
                result = true;
            }
            else if (IsContextBound(entity) == false || ActiveContextIsEntityContext(entity))
            {
                if (HasOwnership(entity, user))
                {
                    result = true;
                }
                else if (IsContextBound(entity) && ActiveContextIsEntityContext(entity))
                {
                    result = true;
                }
                else if (IsUserInMunicipality(user) && HasAssignedPublicAccess(entity))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) && entity.Id == user.Id)
                {
                    result = true;
                }
            }

            return result;
        }

        public bool AllowUpdates(int userId, IEntity entity)
        {
            var result = false;

            var user = _userRepository.GetByKey(userId);
            if (IsGlobalAdmin(user))
            {
                result = true;
            }
            else if (HasAssignedWriteAccess(entity, user))
            {
                result = true;
            }
            else if (IsContextBound(entity) == false || ActiveContextIsEntityContext(entity))
            {
                if (IsLocalAdmin(user))
                {
                    result = true;
                }
                else if (HasModuleLevelWriteAccess(user, entity))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) == false && HasOwnership(entity, user))
                {
                    result = true;
                }
                else if (IsUserEntity(entity) && (entity.Id == user.Id || CanModifyUsers(user)))
                {
                    result = true;
                }
            }

            return result && user.IsReadOnly == false;
        }

        private bool HasModuleLevelWriteAccess(User user, IEntity entity)
        {
            var featureToCheck = default(Feature?);
            switch (entity)
            {
                case IContractModule _:
                    featureToCheck = Feature.CanModifyContracts;
                    break;
                case IOrganizationModule _:
                    featureToCheck = Feature.CanModifyOrganizations;
                    break;
                case IProjectModule _:
                    featureToCheck = Feature.CanModifyProjects;
                    break;
                case ISystemModule _:
                    featureToCheck = Feature.CanModifySystems;
                    break;
                case IReportModule _:
                    featureToCheck = Feature.CanModifyReports;
                    break;
            }

            return featureToCheck.HasValue && _featureChecker.CanExecute(user, featureToCheck.Value);
        }

        private bool CanModifyUsers(User user)
        {
            return _featureChecker.CanExecute(user, Feature.CanModifyUsers);
        }

        private static bool IsUserEntity(IEntity entity)
        {
            return entity is User;
        }

        private static bool HasAssignedPublicAccess(IEntity entity)
        {
            return (entity as IHasAccessModifier)?.AccessModifier == AccessModifier.Public;
        }

        private static bool IsUserInMunicipality(User user)
        {
            return user.DefaultOrganization?.Type?.Category == OrganizationCategory.Municipality;
        }

        private bool TargetOrganizationMatchesActiveOrganization()
        {
            return _authenticationContext.ActiveOrganizationId == _organizationId;
        }

        private static bool IsGlobalAdmin(User user)
        {
            return user.IsGlobalAdmin;
        }

        private static bool HasAssignedWriteAccess(IEntity entity, User user)
        {
            return entity.HasUserWriteAccess(user);
        }

        private static bool IsContextBound(IEntity entity)
        {
            return entity is IContextAware;
        }

        private bool ActiveContextIsEntityContext(IEntity entity)
        {
            return ((IContextAware)entity).IsInContext(_authenticationContext.ActiveOrganizationId.GetValueOrDefault(-1));
        }

        private static bool IsLocalAdmin(User user)
        {
            return user.IsLocalAdmin;
        }

        private static bool HasOwnership(IEntity ownedEntity, IEntity ownerEntity)
        {
            return ownedEntity.ObjectOwnerId == ownerEntity.Id;
        }
    }
}