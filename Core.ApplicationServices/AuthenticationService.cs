﻿using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Security.Authentication;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;

namespace Core.ApplicationServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<User> _userRepository;

        public readonly IFeatureChecker _featureChecker;

        public AuthenticationService(IGenericRepository<User> userRepository, IFeatureChecker featureChecker)
        {
            _userRepository = userRepository;
            _featureChecker = featureChecker;
        }

        public bool IsGlobalAdmin(int userId)
        {
            var user = _userRepository.GetByKey(userId);
            return user.IsGlobalAdmin;
        }

        /// <summary>
        /// Checks if the user is local admin in the current organization.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsLocalAdmin(int userId)
        {
            var user = _userRepository.AsQueryable()
                .SingleOrDefault(x => x.Id == userId &&
                    x.OrganizationRights.Any(
                        right => right.Role == OrganizationRole.LocalAdmin &&
                        right.OrganizationId == x.DefaultOrganizationId));

            return user != null;
        }

        public bool HasReadAccessOutsideContext(int userId)
        {
            var user = _userRepository.GetByKey(userId);

            if (user.IsGlobalAdmin)
                return true;

            // if the user is logged into an organization that allows sharing,
            // then the user have read access outside the context.
            return user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality;
        }

        /// <summary>
        /// Checks if the user have read access to a given instance.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have read access to the given instance, else false.</returns>
        public bool HasReadAccess(int userId, IEntity entity)
        {
            var user = _userRepository.GetByKey(userId);

            var loggedIntoOrganizationId = user.DefaultOrganizationId.GetValueOrDefault(-1);
            if (loggedIntoOrganizationId == -1)
            {
                return false;
            }

            // check if global admin
            if (user.IsGlobalAdmin)
            {
                // global admin always have access
                return true;
            }

            // check if user is object owner
            if (entity.ObjectOwnerId == user.Id)
            {
                // object owners have write access to their objects if they're within the context,
                // else they'll have to switch to the correct context and try again
                return true;
            }

            if (entity is IContextAware) // TODO I don't like this impl
            {
                var awareEntity = entity as IContextAware;

                // check if user is part of the target organization (he's trying to access)
                if (awareEntity.IsInContext(loggedIntoOrganizationId))
                {
                    // users part of an orgaization have read access to all entities inside the organization
                    return true;
                }
                if (user.DefaultOrganization.Type.Category == OrganizationCategory.Municipality)
                {
                    // organizations of type OrganizationCategory.Municipality have read access
                    // to other organizations unless AccessModifier is set to local
                    return (awareEntity as IHasAccessModifier)?.AccessModifier != AccessModifier.Local;

                }

                return false;
            }

            // User is a special case
            //if (entity is User)
            //    return entity.Id == user.Id || _featureChecker.CanExecute(user, Feature.CanModifyUsers);

            return true;
        }

        /// <summary>
        /// Checks if the user have write access to a given instance.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="entity">The instance the user want read access to.</param>
        /// <returns>Returns true if the user have write access to the given instance, else false.</returns>
        public bool HasWriteAccess(int userId, IEntity entity)
        {
            var user = _userRepository.AsQueryable().Single(x => x.Id == userId);
            AssertUserIsNotNull(user);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.GetValueOrDefault(-1);

            if (loggedIntoOrganizationId == -1)
            {
                return false;
            }

            // check if global admin
            if (user.IsGlobalAdmin)
            {
                // global admin always have access
                return true;
            }

            // check "Forretningsroller" for the entity
            if (entity.HasUserWriteAccess(user))
            {
                return true;
            }

            // check ReadOnly
            if (user.IsReadOnly)
            {
                return false;
            }

            //TODO: This checks even if it is not changed. Seems very odd. Also it is too specific. Check should be where it happens....
            //TODO: Add a rule for access modifier check in POST for ITSystem and ITSystemUsage. Then this is covered.
            #region instane
            //Check if user is allowed to set accessmodifier to public
            var accessModifier = (entity as IHasAccessModifier)?.AccessModifier; //TODO: at this stage we cannot assume that the access modifier is "new". Could it not be the old value? It is a strange assumption.
            if (accessModifier == AccessModifier.Public)
            {
                // special case for organisation
                if (entity is Organization) //TODO: IOrganizationController (non-OData( checks for the generic "Can set access modifier"))
                {
                    if (!_featureChecker.CanExecute(user, Feature.CanSetOrganizationAccessModifierToPublic))
                    {
                        return false;
                    }
                }
                //Economy stream can be modified by local admin and contractmodule admin which is why this is a special case.
                if(entity is EconomyStream)
                {
                    if (!_featureChecker.CanExecute(user, Feature.CanSetContractElementsAccessModifierToPublic))
                    {
                        return false;
                    }
                }

                else if (!_featureChecker.CanExecute(user, Feature.CanSetAccessModifierToPublic))
                {
                    return false;
                }
            }
            #endregion insane

            // check if entity is in context
            if (entity is IContextAware) // TODO I don't like this impl
            {
                var awareEntity = (IContextAware)entity;

                // check if entity is part of target organization (he's trying to access)
                if (!awareEntity.IsInContext(loggedIntoOrganizationId))
                {
                    // Users are not allowd to access objects outside their current context,
                    // even if they have access in the other context.
                    // Then they must switch context and try again.
                    return false;
                }
            }

            // check if module admin in target organization and target entity is of the correct type
            if (_featureChecker.CanExecute(user, Feature.CanModifyContracts) && entity is IContractModule)
                return true;

            if (_featureChecker.CanExecute(user, Feature.CanModifyOrganizations) && entity is IOrganizationModule)
                return true;

            if (_featureChecker.CanExecute(user, Feature.CanModifyProjects) && entity is IProjectModule)
                return true;

            if (_featureChecker.CanExecute(user, Feature.CanModifySystems) && entity is ISystemModule)
                return true;

            if (_featureChecker.CanExecute(user, Feature.CanModifyReports) && entity is IReportModule)
                return true;

            // check if user is object owner
            if (entity.ObjectOwner != null && entity.ObjectOwner.Id == user.Id && (entity is IProjectModule || entity is ISystemModule || entity is ItContract || entity is IReportModule))
            {
                // object owners have write access to their objects if they're within the context,
                // else they'll have to switch to the correct context and try again
                return true;
            }            

            // User is a special case
            if (entity is User && (entity.Id == user.Id || _featureChecker.CanExecute(user, Feature.CanModifyUsers)))
                return true;

            // all white-list checks failed, deny access
            return false;
        }

        public int GetCurrentOrganizationId(int userId)
        {
            var user = _userRepository.GetByKey(userId);
            var loggedIntoOrganizationId = user.DefaultOrganizationId.Value;
            return loggedIntoOrganizationId;
        }

        public bool CanExecute(int userId, Feature feature)
        {
            var user = _userRepository.GetByKey(userId);
            return _featureChecker.CanExecute(user, feature);
        }

        // ReSharper disable once UnusedParameter.Local
        private void AssertUserIsNotNull(User user)
        {
            if (user == null)
                throw new AuthenticationException("User is null");
        }
    }
}
