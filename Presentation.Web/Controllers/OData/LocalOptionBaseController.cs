using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using static System.String;

namespace Presentation.Web.Controllers.OData
{
    public class LocalOptionBaseController<TLocalModelType, TDomainModelType, TOptionType> : BaseEntityController<TLocalModelType> where TLocalModelType : LocalOptionEntity<TOptionType> where TOptionType : OptionEntity<TDomainModelType>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGenericRepository<TOptionType> _optionsRepository;

        public LocalOptionBaseController(IGenericRepository<TLocalModelType> repository, IAuthenticationService authService, IGenericRepository<TOptionType> optionsRepository)
            : base(repository, authService)
        {
            _authService = authService;
            _optionsRepository = optionsRepository;
        }

        [EnableQuery]
        public override IHttpActionResult Get()
        {
            if (UserId == 0)
                return Unauthorized();

            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOptionsResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId).ToList();
            var globalOptionsResult = _optionsRepository.AsQueryable().ToList();
            var returnList = new List<TOptionType>();

            foreach (var item in globalOptionsResult)
            {
                if (!item.IsEnabled)
                    continue;

                var itemToAdd = item;
                item.IsActive = false;

                var search = localOptionsResult.Where(x => x.OptionId == item.Id);

                if (search.Any())
                {
                    var localOption = search.First();
                    itemToAdd.IsActive = true;
                    if (!IsNullOrEmpty(localOption.Description))
                    {
                        itemToAdd.Description = localOption.Description;
                    }
                }
                returnList.Add(itemToAdd);
            }
            return Ok(returnList);
        }

        [EnableQuery]
        public override IHttpActionResult Get(int key)
        {
            if (UserId == 0)
                return Unauthorized();

            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var globalOptionResult = _optionsRepository.AsQueryable().Where(x => x.Id == key);

            if (!globalOptionResult.Any())
                return NotFound();

            var option = globalOptionResult.First();

            var localOptionResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == key);

            if (localOptionResult.Any())
            {
                option.IsActive = true;
                var localOption = localOptionResult.First();
                if (!IsNullOrEmpty(localOption.Description))
                {
                    option.Description = localOption.Description;
                }
            }
            else
            {
                option.IsActive = false;
            }

            return Ok(option);
        }

        public override IHttpActionResult Patch(int key, Delta<TLocalModelType> delta)
        {
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOption = Repository.AsQueryable().First(x => x.OrganizationId == orgId && x.OptionId == key);

            // does the entity exist?
            if (localOption == null)
                return NotFound();

            // check if user is allowed to write to the entity
            if (!_authService.HasWriteAccess(UserId, localOption))
                return StatusCode(HttpStatusCode.Forbidden);

            // check model state
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // patch the entity
                delta.Patch(localOption);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            // add the request header "Prefer: return=representation"
            // if you want the updated entity returned,
            // else you'll just get 204 (No Content) returned
            return Updated(localOption);
        }

        public override IHttpActionResult Delete(int globalOptionId)
        {
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            LocalOptionEntity<TOptionType> localOption = Repository.AsQueryable().First(x => x.OrganizationId == orgId && x.OptionId == globalOptionId);

            if (localOption == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, localOption))
                return Unauthorized();

            try
            {
                Repository.DeleteByKey(localOption.Id);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return Ok(localOption.Id);
        }
    }
}
