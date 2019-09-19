using System;
using Core.DomainModel.ItProject;
using Core.DomainServices.Extensions;

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
    }
}
