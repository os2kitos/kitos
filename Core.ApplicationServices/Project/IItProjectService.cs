using System;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.ItProject;
using Core.DomainServices.Queries;


namespace Core.ApplicationServices.Project
{
    public interface IItProjectService
    {
        /// <summary>
        /// Adds an IT project. It creates default phases and saves the project.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Result<ItProject, OperationFailure> AddProject(string name, int organizationId);

        Result<ItProject, OperationFailure> DeleteProject(int id);

        Result<IQueryable<ItProject>, OperationError> GetProjectsInOrganization(Guid organizationUuid, params IDomainQuery<ItProject>[] conditions);

        IQueryable<ItProject> GetAvailableProjects(int organizationId, string optionalNameSearch = null);

        Result<Handover, OperationFailure> AddHandoverParticipant(int projectId, int participantId);

        Result<Handover, OperationFailure> DeleteHandoverParticipant(int projectId, int participantId);
        Result<ItProject, OperationError> GetProject(Guid uuid);
        Result<bool,OperationError> CanCreateNewProjectWithName(string name, int orgId);
        Maybe<OperationError> ValidateNewName(int projectId, string newName);
    }
}
