using System.Collections.Generic;
using Core.DomainModel.ItProject;

namespace Core.DomainServices
{
    public interface IItProjectService
    {
        IEnumerable<ItProject> GetAll(int? orgId = null, string nameSearch = null, bool includePublic = true);

        /// <summary>
        /// Adds an IT project. It creates default phases and saves the project.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        ItProject AddProject(ItProject project);

        void DeleteProject(int id);
    }
}
