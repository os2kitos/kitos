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
        TwoTrackResult<ItProject, GenericOperationFailure> AddProject(ItProject project);

        TwoTrackResult<ItProject, GenericOperationFailure> DeleteProject(int id);

        IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null);
    }
}
