using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class UserController : BaseApiController
    {
        private readonly IGenericRepository<User> _repository;
        private readonly IUserService _userService;
        private readonly IOrgService _orgService;

        public UserController(IGenericRepository<User> repository, IUserService userService, IOrgService orgService)
        {
            _repository = repository;
            _userService = userService;
            _orgService = orgService;
        }

        public HttpResponseMessage Post(UserDTO item)
        {
            try
            {
                if (!IsAuthenticated) return Unauthorized();

                var user = AutoMapper.Mapper.Map<UserDTO, User>(item);

                user = _userService.AddUser(user);

                return Created(AutoMapper.Mapper.Map<User, UserDTO>(user), new Uri(Request.RequestUri + "/" + user.Id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get()
        {
            try
            {
                var users = _repository.Get();
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, List<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get(int id)
        {
            try
            {
                var user = _repository.GetByKey(id);
                return Ok(AutoMapper.Mapper.Map<User, UserDTO>(user));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Get(bool? organizations)
        {
            try
            {
                var user = KitosUser;

                var orgs = _orgService.GetByUser(user);
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

        public HttpResponseMessage Get(string q)
        {
            if (string.IsNullOrEmpty(q)) return Get();

            try
            {
                var users = _repository.Get(u => u.Name.StartsWith(q) || u.Email.StartsWith(q));
                return Ok(AutoMapper.Mapper.Map<IEnumerable<User>, IEnumerable<UserDTO>>(users));
            }
            catch (Exception e)
            {
                return Error(e);
            }
            
        }

        public virtual HttpResponseMessage Patch(int id, JObject obj)
        {
            var item = _repository.GetByKey(id);
            var itemType = item.GetType();

            foreach (var valuePair in obj)
            {
                // get name of mapped property
                var map =
                    AutoMapper.Mapper.FindTypeMapFor<UserProfileDTO, User>()
                              .GetPropertyMaps();
                var nonNullMaps = map.Where(x => x.SourceMember != null);
                var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                if (mapMember == null)
                    continue; // abort if no map found

                var destName = mapMember.DestinationProperty.Name;
                var jToken = valuePair.Value;

                var propRef = itemType.GetProperty(destName);
                var t = propRef.PropertyType;
                // use reflection to call obj.Value<t>("keyName");
                var genericMethod = jToken.GetType().GetMethod("Value").MakeGenericMethod(new Type[] { t });
                var value = genericMethod.Invoke(obj, new object[] { valuePair.Key });
                // update the entity
                propRef.SetValue(item, value);
            }

            try
            {
                _repository.Update(item);
                _repository.Save();

                //pretty sure we'll get a merge conflict here???
                return Ok(AutoMapper.Mapper.Map<User, UserProfileDTO>(item)); // TODO correct?
            }
            catch (Exception)
            {
                return NoContent(); // TODO catch correct expection
            }
        }
    }

}
