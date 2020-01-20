using System;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainServices.Extensions;
using Core.DomainServices.Model;

namespace Core.DomainServices.Repositories.Project
{
    public class ItProjectRepository : IItProjectRepository
    {
        private readonly IGenericRepository<ItProject> _repository;

        public ItProjectRepository(IGenericRepository<ItProject> repository)
        {
            _repository = repository;
        }

        public ItProject GetById(int id)
        {
            return _repository.AsQueryable().ById(id);
        }

        public IQueryable<ItProject> GetProjects(OrganizationDataQueryParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            return _repository.AsQueryable().ByOrganizationDataQueryParameters(parameters);
        }
    }
}
