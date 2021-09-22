using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItProject;


namespace Core.DomainServices.Repositories.Project
{
    public interface IItProjectRepository
    {
        ItProject GetById(int id);
        IQueryable<ItProject> GetProjectsInOrganization(int organizationId);
        IQueryable<ItProject> GetProjects();
        Maybe<ItProject> GetProject(Guid uuid);
    }
}
