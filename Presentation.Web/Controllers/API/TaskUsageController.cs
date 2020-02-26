using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Newtonsoft.Json.Linq;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Controller.Crud;
using Presentation.Web.Models;
using Swashbuckle.Swagger.Annotations;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class TaskUsageController : GenericHierarchyApiController<TaskUsage, TaskUsageDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;
        private readonly IGenericRepository<TaskRef> _taskRepository;

        public TaskUsageController(
            IGenericRepository<TaskUsage> repository,
            IGenericRepository<OrganizationUnit> orgUnitRepository,
            IGenericRepository<TaskRef> taskRepository)
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
            _taskRepository = taskRepository;
        }

        protected override IControllerCrudAuthorization GetCrudAuthorization()
        {
            return new ChildEntityCrudAuthorization<TaskUsage, OrganizationUnit>(x => _orgUnitRepository.GetByKey(x.OrgUnitId), base.GetCrudAuthorization());
        }

        [HttpGet]
        [Route("api/taskUsage/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ApiReturnDTO<List<TaskUsageNestedDTO>>))]
        public HttpResponseMessage Get(int orgUnitId, int organizationId, bool onlyStarred, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            try
            {
                if (GetOrganizationReadAccessLevel(organizationId) < OrganizationDataReadAccessLevel.All)
                {
                    return Forbidden();
                }

                pagingModel.Where(usage => usage.OrgUnitId == orgUnitId);

                if (onlyStarred) pagingModel.Where(usage => usage.Starred);

                var usages = Page(Repository.AsQueryable(), pagingModel).ToList();

                var dtos = new List<TaskUsageNestedDTO>();

                foreach (var taskUsage in usages)
                {
                    var dto = Map<TaskUsage, TaskUsageNestedDTO>(taskUsage);
                    dto.HasWriteAccess = AllowModify(taskUsage);
                    dto.SystemUsages = AssociatedSystemUsages(taskUsage);
                    dto.Projects = AssociatedProjects(taskUsage);
                    dtos.Add(dto);
                }

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [HttpPost]
        [Route("api/taskUsage/taskGroup")]
        public HttpResponseMessage PostTaskGroup(int orgUnitId, int? taskId)
        {
            try
            {
                var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);
                if (orgUnit == null)
                    return NotFound();
                List<TaskRef> tasks;
                if (taskId.HasValue)
                {
                    // get child leaves of taskId that havn't got a usage in the org unit
                    tasks = _taskRepository.Get(
                        x =>
                            (x.ParentId == taskId || x.Parent.ParentId == taskId) && !x.Children.Any() &&
                            x.Usages.All(y => y.OrgUnitId != orgUnitId)).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    tasks = _taskRepository.Get(
                        x =>
                            !x.Children.Any() &&
                            x.Usages.All(y => y.OrgUnitId != orgUnitId)).ToList();
                }

                if (!tasks.Any())
                    return NotFound();

                foreach (var task in tasks)
                {
                    var taskUsage = new TaskUsage()
                    {
                        OrgUnitId = orgUnitId,
                        TaskRefId = task.Id,
                        ObjectOwner = KitosUser,
                        LastChanged = DateTime.UtcNow,
                        LastChangedByUser = KitosUser
                    };
                    if (!AllowCreate<TaskUsage>(taskUsage))
                    {
                        return Forbidden();
                    }
                    Repository.Insert(taskUsage);
                }
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [HttpPost]
        [Route("api/taskUsage/")]
        public override HttpResponseMessage Post(TaskUsageDTO taskUsageDto)
        {
            try
            {
                var item = Map<TaskUsageDTO, TaskUsage>(taskUsageDto);
                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;
                if (!AllowCreate<TaskUsage>(item))
                {
                    return Forbidden();
                }
                var savedItem = PostQuery(item);

                return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
            }
            catch (Exception e)
            {
                // check if inner message is a duplicate, if so return conflict
                if (e.InnerException?.InnerException != null)
                {
                    if (e.InnerException.InnerException.Message.Contains("Duplicate entry"))
                    {
                        return Conflict(e.InnerException.InnerException.Message);
                    }
                }
                return LogError(e);
            }
        }

        [HttpDelete]
        [Route("api/taskUsage/")]
        public HttpResponseMessage DeleteTaskGroup(int orgUnitId, int? taskId)
        {
            try
            {
                var orgUnit = _orgUnitRepository.GetByKey(orgUnitId);
                if (orgUnit == null)
                    return NotFound();

                List<TaskUsage> taskUsages;
                if (taskId.HasValue)
                {
                    taskUsages = orgUnit.TaskUsages.Where(
                        taskUsage => taskUsage.TaskRef.ParentId == taskId || taskUsage.TaskRef.Parent.ParentId == taskId).ToList();
                }
                else
                {
                    // no taskId was specified so get everything
                    taskUsages = orgUnit.TaskUsages.ToList();
                }

                if (!taskUsages.Any())
                    return NotFound();

                foreach (var taskUsage in taskUsages)
                {
                    if (!AllowDelete(taskUsage))
                    {
                        return Forbidden();
                    }
                    Repository.DeleteByKey(taskUsage.Id);
                }
                Repository.Save();
                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        [HttpGet]
        [Route("api/taskUsage/")]
        public HttpResponseMessage GetExcel(bool? csv, int orgUnitId, bool onlyStarred)
        {
            try
            {
                var usages = Repository
                    .Get(usage => usage.OrgUnitId == orgUnitId && usage.Starred == onlyStarred)
                    .Where(AllowRead);

                var dtos = new List<TaskUsageNestedDTO>();

                foreach (var taskUsage in usages)
                {
                    var dto = Map<TaskUsage, TaskUsageNestedDTO>(taskUsage);
                    dto.SystemUsages = AssociatedSystemUsages(taskUsage);
                    dto.Projects = AssociatedProjects(taskUsage);
                    dtos.Add(dto);
                }

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("OrgUnit", "Organisationsenhed");
                header.Add("KLEID", "KLE ID");
                header.Add("KLENavn", "KLE Navn");
                header.Add("Teknologi", "Teknologi");
                header.Add("Anvendelse", "Anvendelse");
                header.Add("Kommentar", "Kommentar");
                header.Add("Projekt", "IT Projekt");
                header.Add("System", "IT System");
                list.Add(header);

                // Adding via recursive method so that children are
                // placed directly after their parent in the output
                Action<TaskUsageNestedDTO> addUsageToList = null;
                addUsageToList = elem =>
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("OrgUnit", elem.OrgUnitName);
                    obj.Add("KLEID", elem.TaskRefTaskKey);
                    obj.Add("KLENavn", elem.TaskRefDescription);
                    obj.Add("Teknologi", elem.TechnologyStatus);
                    obj.Add("Anvendelse", elem.UsageStatus);
                    obj.Add("Kommentar", elem.Comment);
                    obj.Add("Projekt", String.Join(",", elem.Projects.Select(x => x.Name)));
                    obj.Add("System", String.Join(",", elem.SystemUsages.Select(x => x.ItSystemName)));
                    list.Add(obj);
                    foreach (var child in elem.Children)
                    {
                        addUsageToList(child); // recursive call
                    }
                };

                foreach (var usage in dtos)
                {
                    addUsageToList(usage);
                }
                var s = list.ToCsv();
                var bytes = Encoding.Unicode.GetBytes(s);
                var stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "organisationsoverblik.csv", DispositionType = "ISO-8859-1" };
                return result;
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        protected override TaskUsage PostQuery(TaskUsage item)
        {
            var orgUnit = _orgUnitRepository.GetByKey(item.OrgUnitId);

            if (orgUnit.ParentId != null)
            {
                var parentUsage = orgUnit.Parent.TaskUsages.First(usage => usage.TaskRefId == item.TaskRefId);
                item.Parent = parentUsage;
            }

            return base.PostQuery(item);
        }

        public override HttpResponseMessage Put(int id, int organizationId, JObject jObject)
        {
            return NotAllowed();
        }

        private IEnumerable<ItSystemUsageSimpleDTO> AssociatedSystemUsages(TaskUsage taskUsage)
        {
            var indirectUsages =
                    taskUsage.TaskRef.ItSystems.SelectMany(system => system.Usages)
                             .Where(usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

            var directUsages =
                taskUsage.TaskRef.ItSystemUsages.Where(
                    usage => usage.OrganizationId == taskUsage.OrgUnit.OrganizationId);

            var allUsages = indirectUsages.Union(directUsages);

            return Map<IEnumerable<ItSystemUsage>, IEnumerable<ItSystemUsageSimpleDTO>>(allUsages);
        }

        private IEnumerable<ItProjectSimpleDTO> AssociatedProjects(TaskUsage taskUsage)
        {
            var theProjects = taskUsage.TaskRef.ItProjects.Where(p => p.OrganizationId == taskUsage.OrgUnit.OrganizationId);
            return Map<IEnumerable<ItProject>, IEnumerable<ItProjectSimpleDTO>>(theProjects);
        }
    }
}
