using System.Linq;
using Core.DomainModel.ItProject;
using Core.DomainServices.Model;

namespace Core.DomainServices.Repositories.Project
{
    public interface IItProjectRepository
    {
        ItProject GetById(int id);
        IQueryable<ItProject> GetProjects(OrganizationDataQueryParameters parameters);
    }
}
