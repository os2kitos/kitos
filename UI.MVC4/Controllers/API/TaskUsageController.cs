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
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class TaskUsageController : GenericHierarchyApiController<TaskUsage, TaskUsageDTO>
    {
        private readonly IGenericRepository<OrganizationUnit> _orgUnitRepository;

        public TaskUsageController(IGenericRepository<TaskUsage> repository, IGenericRepository<OrganizationUnit> orgUnitRepository) 
            : base(repository)
        {
            _orgUnitRepository = orgUnitRepository;
        }

        public HttpResponseMessage Get(int orgUnitId, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            return Get(orgUnitId, false, pagingModel);
        }

        public HttpResponseMessage Get(int orgUnitId, bool onlyStarred, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            try
            {
                pagingModel.Where(usage => usage.OrgUnitId == orgUnitId);

                if (onlyStarred) pagingModel.Where(usage => usage.Starred);

                var usages = Page(Repository.AsQueryable(), pagingModel).ToList();

                var dtos = new List<TaskUsageNestedDTO>();

                foreach (var taskUsage in usages)
                {
                    var dto = Map<TaskUsage, TaskUsageNestedDTO>(taskUsage);
                    dto.HasWriteAccess = HasWriteAccess(taskUsage, KitosUser);
                    dto.SystemUsages = AssociatedSystemUsages(taskUsage);
                    dto.Projects = AssociatedProjects(taskUsage);
                    dtos.Add(dto);
                }
                
                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetExcel(bool? csv, int orgUnitId, bool onlyStarred, [FromUri] PagingModel<TaskUsage> pagingModel)
        {
            try
            {
                pagingModel.Where(usage => usage.OrgUnitId == orgUnitId);

                if (onlyStarred) pagingModel.Where(usage => usage.Starred);

                var usages = Page(Repository.AsQueryable(), pagingModel).ToList();

                var dtos = new List<TaskUsageNestedDTO>();

                foreach (var taskUsage in usages)
                {
                    var dto = Map<TaskUsage, TaskUsageNestedDTO>(taskUsage);
                    dto.HasWriteAccess = HasWriteAccess(taskUsage, KitosUser);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "itsystemanvendelsesoversigt.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
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
        
        public override HttpResponseMessage Put(int id, JObject jObject)
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
