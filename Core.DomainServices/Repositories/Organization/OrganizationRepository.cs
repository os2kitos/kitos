using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainServices.Extensions;


namespace Core.DomainServices.Repositories.Organization
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IGenericRepository<DomainModel.Organization.Organization> _genericRepository;

        public OrganizationRepository(IGenericRepository<DomainModel.Organization.Organization> repository)
        {
            _genericRepository = repository;
        }

        public IQueryable<DomainModel.Organization.Organization> GetAll()
        {
            return _genericRepository.AsQueryable();
        }

        public Maybe<DomainModel.Organization.Organization> GetById(int id)
        {
            return _genericRepository.AsQueryable().ById(id);
        }

        public Maybe<DomainModel.Organization.Organization> GetByCvr(string cvrNumber)
        {
            return _genericRepository
                .AsQueryable()
                .Where(organization => organization.Cvr == cvrNumber)
                .FirstOrDefault();
        }

        public Maybe<DomainModel.Organization.Organization> GetByUuid(Guid uuid)
        {
            return _genericRepository
                .AsQueryable()
                .ByUuid(uuid);
        }

        public void Update(DomainModel.Organization.Organization organization)
        {
            _genericRepository.Update(organization);
            _genericRepository.Save();
        }
    }
}
