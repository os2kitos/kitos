using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Core.ApplicationServices
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization> _orgRepository;
        private readonly IGenericRepository<OrganizationRight> _orgRightRepository;

        public OrganizationService(IGenericRepository<Organization> orgRepository, IGenericRepository<OrganizationRight> orgRightRepository)
        {
            _orgRepository = orgRepository;
            _orgRightRepository = orgRightRepository;
        }

        /// <summary>
        /// lists the organizations the user is a member of
        /// </summary>
        /// <param name="user"></param>
        /// <returns>a list of organizations that the user is a member of</returns>
        public IEnumerable<Organization> GetOrganizations(User user)
        {
            if (user.IsGlobalAdmin) return _orgRepository.Get();
            return _orgRepository.Get(o => o.Rights.Any(r => r.OrganizationId == o.Id && r.UserId == user.Id));
        }

        //returns the default org unit for that user inside that organization
        //or null if none has been chosen
        public OrganizationUnit GetDefaultUnit(Organization organization, User user)
        {
            return _orgRightRepository.Get(r => r.OrganizationId == organization.Id && r.UserId == user.Id).Select(r => r.DefaultOrgUnit).FirstOrDefault();
        }

        public void SetDefaultOrgUnit(User user, int orgId, int orgUnitId)
        {
            //TODO this should probably be Single() ?
            var right = _orgRightRepository.Get(r => r.UserId == user.Id && r.OrganizationId == orgId).First();
            right.DefaultOrgUnitId = orgUnitId;

            _orgRightRepository.Update(right);
            _orgRightRepository.Save();
        }

        public void SetupDefaultOrganization(Organization org, User objectOwner)
        {
            org.Config = Config.Default(objectOwner);
            org.OrgUnits.Add(new OrganizationUnit()
                {
                    Name = org.Name,
                    ObjectOwnerId = objectOwner.Id,
                    LastChangedByUserId = objectOwner.Id
                });
        }

        /// <summary>
        /// Remove all organization rights from a user.
        /// </summary>
        /// <param name="organizationId">The organization the user should be removed from.</param>
        /// <param name="userId">The user to be removed.</param>
        public void RemoveUser(int organizationId, int userId)
        {
            var rights = _orgRightRepository.Get(x => x.UserId == userId && x.OrganizationId == organizationId);
            foreach (var right in rights)
            {
                _orgRightRepository.DeleteByKey(right.Id);
            }
            _orgRightRepository.Save();
        }

        public void addContactPerson(int organizationId, int contactId)
        {
            var org = _orgRepository.Get(o => o.Id == organizationId).First();
            org.ContactPersonId = contactId;
            _orgRepository.Update(org);
            _orgRepository.Save();
        }
    }
}
