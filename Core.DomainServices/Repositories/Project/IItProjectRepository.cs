using System;
using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainServices.Model;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Repositories.Project
{
    public interface IItProjectRepository
    {
        ItProject GetById(int id);
        IQueryable<ItProject> GetProjects(OrganizationDataQueryParameters parameters);
        IQueryable<ItProject> GetProjects();
        Maybe<ItProject> GetProject(Guid uuid);
    }
}
