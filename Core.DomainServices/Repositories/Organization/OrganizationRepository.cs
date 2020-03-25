using System;
using System.Linq;
using Core.DomainModel.Result;

namespace Core.DomainServices.Repositories.Organization
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IGenericRepository<DomainModel.Organization.Organization> _repository;

        public OrganizationRepository(IGenericRepository<DomainModel.Organization.Organization> repository)
        {
            _repository = repository;
        }

        public Maybe<DomainModel.Organization.Organization> GetByCvr(string cvrNumber)
        {
            return _repository
                .AsQueryable()
                .Where(organization => organization.Cvr == cvrNumber)
                .FirstOrDefault();
        }
    }
}
