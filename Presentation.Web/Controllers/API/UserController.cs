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
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using Ninject;
using Ninject.Web.Common;
using Presentation.Web.Models;
using WebGrease.Css.Extensions;

namespace Presentation.Web.Controllers.API
{
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

            //todo: this is bad crosscutting of concerns. refactor / extract into separate controller
            _kernel = kernel; //we need this for retrieving userroles when creating a csv file.
        }

        public override HttpResponseMessage Post(UserDTO dto)
        {
            try
            {
                //do some string magic to determine parameters, and actions
                List<string> parameters = null;
                var sendMailOnCreation = false;
                var sendReminder = false;
                var sendAdvis = false;

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
                    }
                }

                //check if user already exists and we are not sending a reminder or advis. If so, just return him
                var existingUser = Repository.Get(u => u.Email == dto.Email).FirstOrDefault();
                if (existingUser != null && !sendReminder && !sendAdvis)
                    return Ok(Map(existingUser));
                //if we are sending a reminder:
                if (existingUser != null && sendReminder)
                {
                    _userService.IssueAdvisMail(existingUser, true);
                    return Ok(Map(existingUser));
                }
                //if we are sending an advis:
                if (existingUser != null && sendAdvis)
                {
                    _userService.IssueAdvisMail(existingUser, false);
                    return Ok(Map(existingUser));
                }

                //otherwise we are creating a new user
                var item = Map(dto);

                item.ObjectOwner = KitosUser;
                item.LastChangedByUser = KitosUser;

                item = _userService.AddUser(item, sendMailOnCreation);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        //TODO REWRITE THIS, perhaps so it's passed along at login?
        public HttpResponseMessage Get(bool? organizations)
        {
            try
            {
                var user = KitosUser;

                var orgs = _organizationService.GetByUser(user);
                var dtos = AutoMapper.Mapper.Map<ICollection<Organization>, ICollection<OrganizationDTO>>(orgs);

                //if the user has selected a default org unit, use the responding organization as default organization
                var defaultOrgId = (user.DefaultOrganizationUnit == null)
                                       ? 0
                                       : user.DefaultOrganizationUnit.OrganizationId;

                var result = new UserOrganizationsDTO()
                    {
                        Organizations = dtos,
                        DefaultOrganizationId = defaultOrgId
                    };

                return Ok(result);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetBySearch(string q)
        {
            try
            {
                var users = Repository.Get(u => u.Name.Contains(q) || u.Email.Contains(q));
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
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

                //Get all users inside the organization
                //pagingModel.Where(u => u.CreatedInId == orgId);

                pagingModel.Where(u => u.AdminRights.Count(r => r.ObjectId == orgId) != 0 );

                var users = Page(Repository.AsQueryable(), pagingModel);


                return Ok(Map(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage GetExcel([FromUri] bool? csv, [FromUri] int orgId)
        {
            try
            {
                var users = Repository.Get(u => u.CreatedInId == orgId);

                var dtos = Map(users);

                var list = new List<dynamic>();
                var header = new ExpandoObject() as IDictionary<string, Object>;
                header.Add("Status", "Status");
                header.Add("Navn", "Navn");
                header.Add("Email", "Email");
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
                    obj.Add("Status", user.IsLocked ? "Låst" : "Ikke låst");
                    obj.Add("Navn", user.Name);
                    obj.Add("Email", user.Email);
                    obj.Add("Organisationsenhed", user.DefaultOrganizationUnitName);
                    obj.Add("Advis", user.LastAdvisDate.HasValue ? user.LastAdvisDate.Value.ToString("dd-MM-yy") : "Ikke sendt");
                    obj.Add("Oprettet", user.ObjectOwnerName);
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
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = "brugerkatalog.csv" };
                return result;
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        #region GetRights
        private string GetOrgRights(int orgId, int userId)
        {
            var rightsRepository = _kernel.Get<IGenericRepository<OrganizationRight>>();
            var orgUnitService = _kernel.Get<IOrgUnitService>();

            var orgUnits = orgUnitService.GetSubTree(orgId);

            var theRights = new List<OrganizationRight>();
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

        public HttpResponseMessage GetNameAvailable(string checkname, int orgId)
        {
            try
            {
                return IsAvailable(checkname, orgId) ? Ok() : Conflict("Name is already taken!");
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        private bool IsAvailable(string email, int orgId)
        {
            var users = Repository.Get(u => u.Email == email && u.CreatedInId == orgId);
            return !users.Any();
        }
    }

}
