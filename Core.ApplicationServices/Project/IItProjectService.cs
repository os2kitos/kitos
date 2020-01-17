using System.Linq;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItProject;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Project
{
    public interface IItProjectService
    {
        /// <summary>
        /// Adds an IT project. It creates default phases and saves the project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Result<ItProject, OperationFailure> AddProject(ItProject project);

        Result<ItProject, OperationFailure> DeleteProject(int id);

        IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null);
    }
}
