using System;
using System.Linq;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Organization
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IGenericRepository<DomainModel.Organization.Organization> _repository;

        public OrganizationRepository(IGenericRepository<DomainModel.Organization.Organization> repository)
        {
            _repository = repository;
        }

        public IQueryable<DomainModel.Organization.Organization> GetAll()
        {
            return _repository.AsQueryable();
        }

        public Maybe<DomainModel.Organization.Organization> GetById(int id)
        {
            return _repository.AsQueryable().ById(id);
        }

        public Maybe<DomainModel.Organization.Organization> GetByCvr(string cvrNumber)
        {
            return _repository
                .AsQueryable()
                .Where(organization => organization.Cvr == cvrNumber)
                .FirstOrDefault();
        }

        public Maybe<DomainModel.Organization.Organization> GetByUuid(Guid uuid)
        {
            return _repository
                .AsQueryable()
                .Where(organization => organization.Uuid == uuid)
                .FirstOrDefault();
        }
    }
}
