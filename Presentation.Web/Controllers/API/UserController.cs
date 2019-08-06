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
using AutoMapper;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Ninject;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [InternalApi]
    public class UserController : GenericApiController<User, UserDTO>
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IKernel _kernel;

        public UserController(IGenericRepository<User> repository, IUserService userService, IOrganizationService organizationService, IKernel kernel)
            : base(repository)
        {
            _userService = userService;
            _organizationService = organizationService;

            // TODO: this is bad crosscutting of concerns. refactor / extract into separate controller
            _kernel = kernel; // we need this for retrieving userroles when creating a csv file.
        }

        public override HttpResponseMessage Post(UserDTO dto)
        {
            try
            {
                // do some string magic to determine parameters, and actions
                List<string> parameters = null;
                var sendMailOnCreation = false;
                var sendReminder = false;
                var sendAdvis = false;
                int? orgId = null;

                if (!string.IsNullOrWhiteSpace(Request.RequestUri.Query))
                    parameters = new List<string>(Request.RequestUri.Query.Replace("?", string.Empty).Split('&'));
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("sendMailOnCreation"))
                        {
                            sendMailOnCreation = bool.Parse(parameter.Replace("sendMailOnCreation=", string.Empty));
                        }
                        if (parameter.StartsWith("sendReminder"))
                        {
                            sendReminder = bool.Parse(parameter.Replace("sendReminder=", string.Empty));
                        }
                        if (parameter.StartsWith("sendAdvis"))
                        {
                            sendAdvis = bool.Parse(parameter.Replace("sendAdvis=", string.Empty));
                        }
                        if (parameter.StartsWith("organizationId="))
                        {
                            orgId = int.Parse(parameter.Replace("organizationId=", string.Empty));
                        }
                    }
                }

                // check if orgId is set, if not return error as we cannot continue without it
                if (!orgId.HasValue)
                {
                    return Error("Organization id is missing!");
                }

                // only global admin is allowed to set others to global admin
                if (dto.IsGlobalAdmin && !KitosUser.IsGlobalAdmin)
                {
                    return Forbidden();
                }

                // check if user already exists and we are not sending a reminder or advis. If so, just return him
                var existingUser = Repository.Get(u => u.Email == dto.Email).FirstOrDefault();
                if (existingUser != null && !sendReminder && !sendAdvis)
                    return Ok(Map(existingUser));
                // if we are sending a reminder:
                if (existingUser != null && sendReminder)
                {
                    _userService.IssueAdvisMail(existingUser, true, orgId.Value);
                    return Ok(Map(existingUser));
                }
                // if we are sending an advis:
                if (existingUser != null && sendAdvis)
                {
                    _userService.IssueAdvisMail(existingUser, false, orgId.Value);
                    return Ok(Map(existingUser));
                }

                // otherwise we are creating a new user
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                item = _userService.AddUser(item, sendMailOnCreation, orgId.Value);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            // get name of mapped property
            var map = Mapper.FindTypeMapFor<UserDTO, User>().GetPropertyMaps();
            var nonNullMaps = map.Where(x => x.SourceMember != null).ToList();

            foreach (var valuePair in obj)
            {
                var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                if (mapMember == null)
                    continue; // abort if no map found

                var destName = mapMember.DestinationProperty.Name;

                if (destName == "IsGlobalAdmin")
                    if (valuePair.Value.Value<bool>()) // check if value is being set to true
                        if (!KitosUser.IsGlobalAdmin)
                            return Forbidden(); // don't allow users to elevate to global admin unless done by a global admin
            }

            return base.Patch(id, organizationId, obj);
        }

        //public HttpResponseMessage PostTokenRequest(bool? token, int userId)
        //{
        //    try
        //    {
        //        var user = Repository.GetByKey(userId);
        //        if (user == null)
        //            return NotFound();

        //        user.UniqueId = Guid.NewGuid();
        //        PatchQuery(user, null);
        //        return Ok(user.Uuid);
        //    }
        //    catch (Exception e)
        //    {
        //        return LogError(e);
        //    }
        //}

        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var users = Repository.Get(u => u.Name.Contains(q) || u.Email.Contains(q));
                return Ok(Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetByOrganization(int orgId, bool? usePaging, [FromUri] PagingModel<User> pagingModel, [FromUri] string q)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(q))
                    pagingModel.Where(u =>
                        u.Name.Contains(q)
                        || u.Email.Contains(q));

                // Get all users inside the organization
                pagingModel.Where(u => u.OrganizationRights.Count(r => r.Role == OrganizationRole.User && r.OrganizationId == orgId) > 0);

                var users = Page(Repository.AsQueryable(), pagingModel).ToList();

                return Ok(Map(users));
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetOverview(bool? overview, int orgId, [FromUri] PagingModel<User> pagingModel, [FromUri] string q)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(q))
                    pagingModel.Where(u =>
                        u.Name.Contains(q)
                        || u.Email.Contains(q));

                // Get all users inside the organization
                pagingModel.Where(u => u.OrganizationRights.Count(r => r.OrganizationId == orgId) > 0);

                var users = Page(Repository.AsQueryable(), pagingModel).ToList();
                var dtos = new List<UserOverviewDTO>();

                foreach (var user in users)
                {
                    var newDTO = Map<User, UserOverviewDTO>(user);

                    newDTO.CanBeEdited = HasWriteAccess(user, KitosUser, orgId);
                    dtos.Add(newDTO);
                }

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage GetExcel([FromUri] bool? csv, [FromUri] int orgId)
        {
            try
            {
                var users = Repository.Get(u => u.OrganizationRights.Count(r => r.OrganizationId == orgId) != 0);

                var dtos = Map(users);

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Fornavn", "Fornavn");
                header.Add("Efternavn", "Efternavn");
                header.Add("Email", "Email");
                header.Add("DefaultUserStartPreference", "DefaultUserStartPreference");
                header.Add("Organisationsenhed", "Default org.enhed");
                header.Add("Advis", "Advis");
                header.Add("Oprettet", "Oprettet Af");
                header.Add("OrgRoller", "Organisations roller"); 
                header.Add("ITProjektRoller", "ITProjekt roller");
                header.Add("ITSystemRoller", "ITSystem roller");
                header.Add("ITKontraktRoller", "ITKontrakt roller");
                list.Add(header);

                foreach (var user in dtos)
                {
                    var obj = new ExpandoObject() as IDictionary<string, Object>;
                    obj.Add("Fornavn", user.Name);
                    obj.Add("Efternavn", user.LastName);
                    obj.Add("Email", user.Email);
                    obj.Add("DefaultUserStartPreference", user.DefaultUserStartPreference);
                    obj.Add("Organisationsenhed", user.DefaultOrganizationUnitName);
                    obj.Add("Advis", user.LastAdvisDate.HasValue ? user.LastAdvisDate.Value.ToString("dd-MM-yy") : "Ikke sendt");
                    obj.Add("Oprettet", user.ObjectOwnerName + " " + user.ObjectOwnerLastName);
                    obj.Add("OrgRoller", GetOrgRights(orgId, user.Id));
                    obj.Add("ITProjektRoller", GetProjectRights(user.Id));
                    obj.Add("ITSystemRoller", GetSystemRights(user.Id));
                    obj.Add("ITKontraktRoller", GetContractRights(user.Id));
                    list.Add(obj);
                }

                var csvList = list.ToCsv();
                var bytes = Encoding.Unicode.GetBytes(csvList);
                var stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                var result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = "brugerkatalog.csv", DispositionType = "ISO-8859-1" };
                return result;
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        #region GetRights

        private string GetOrgRights(int orgId, int userId)
        {
            var rightsRepository = _kernel.Get<IGenericRepository<OrganizationUnitRight>>();
            var orgUnitService = _kernel.Get<IOrgUnitService>();

            var orgUnits = orgUnitService.GetSubTree(orgId);

            var theRights = new List<OrganizationUnitRight>();
            foreach (var orgUnit in orgUnits)
            {
                var id = orgUnit.Id;
                theRights.AddRange(rightsRepository.Get(r => r.ObjectId == id && r.UserId == userId));
            }
            var dtos = Mapper.Map<List<RightOutputDTO>>(theRights);

            return StringifyRights(dtos);
        }

        private string GetProjectRights(int userId)
        {
            var rightsRepository = _kernel.Get<IGenericRepository<ItProjectRight>>();

            var theRights = rightsRepository.Get(r => r.UserId == userId);

            return StringifyRights(Mapper.Map<List<RightOutputDTO>>(theRights));
        }

        private string GetSystemRights(int userId)
        {
            var rightsRepository = _kernel.Get<IGenericRepository<ItSystemRight>>();

            var theRights = rightsRepository.Get(r => r.UserId == userId);

            return StringifyRights(Mapper.Map<List<RightOutputDTO>>(theRights));
        }

        private string GetContractRights(int userId)
        {
            var rightsRepository = _kernel.Get<IGenericRepository<ItContractRight>>();

            var theRights = rightsRepository.Get(r => r.UserId == userId);

            return StringifyRights(Mapper.Map<List<RightOutputDTO>>(theRights));
        }

        private static string StringifyRights(List<RightOutputDTO> dtos)
        {
            var builder = new StringBuilder();
            foreach (var dto in dtos)
            {
                builder.Append(dto.ObjectName).Append(':').Append(dto.RoleName);
                if (dtos.Last() != dto)
                    builder.Append(',');
            }
            return builder.ToString();
        }

        #endregion

        public HttpResponseMessage GetNameIsAvailable(string checkname, int orgId)
        {
            try
            {
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        private bool IsAvailable(string email, int orgId)
        {
            var users = Repository.Get(u => u.Email == email);

            return !users.Any();
        }

        public HttpResponseMessage GetUserExistsWithRole(string email, int orgId, bool? userExistsWithRole)
        {
            var users = Repository.Get(u => u.Email == email && u.OrganizationRights.Count(r => r.Role == OrganizationRole.User && r.OrganizationId == orgId) > 0);

            if (users.Any()) return Ok();

            return NotFound();
        }

        public HttpResponseMessage PostDefaultOrgUnit(bool? updateDefaultOrgUnit, UpdateDefaultOrgUnitDto dto)
        {
            try
            {
                _organizationService.SetDefaultOrgUnit(KitosUser, dto.OrgId, dto.OrgUnitId);

                return Ok();
            }
            catch (Exception e)
            {
                return LogError(e);
            }
        }

        public HttpResponseMessage PostDefaultOrganization(bool? updateDefaultOrganization, int organizationId)
        {
            var userId = int.Parse(User.Identity.Name);
            var user = Repository.Get(x => x.Id == userId).First();
            user.DefaultOrganizationId = organizationId;
            Repository.Save();
            return Ok();
        }

        protected override bool HasWriteAccess(User obj, User user, int organizationId)
        {
            //if user is readonly
            if (user.IsReadOnly && !user.IsGlobalAdmin)
                return false;

            return base.HasWriteAccess(obj, user, organizationId);
        }

        /// <summary>
        /// Deletes user from the system
        /// </summary>
        /// <param name="id">The id of the user to be deleted</param>
        /// <param name="organizationId">Not used in this case. Should remain empty</param>
        /// <returns></returns>
        public override HttpResponseMessage Delete(int id, int organizationId = 0)
        {
            if (!KitosUser.OrganizationRights.Any(x => x.Role == OrganizationRole.GlobalAdmin || x.Role == OrganizationRole.LocalAdmin || x.Role == OrganizationRole.OrganizationModuleAdmin))
            {
                return Forbidden();
            }

            return base.Delete(id, organizationId);
        }
    }
}
