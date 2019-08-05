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
    public class LocalOptionBaseController<TLocalModelType, TDomainModelType, TOptionType> : BaseEntityController<TLocalModelType> where TLocalModelType : LocalOptionEntity<TOptionType>, new() where TOptionType : OptionEntity<TDomainModelType>
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
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOptionsResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId).ToList();
            var globalOptionsResult = _optionsRepository.AsQueryable().ToList();
            var returnList = new List<TOptionType>();

            foreach (var item in globalOptionsResult)
            {
                if (!item.IsEnabled)
                    continue;

                var itemToAdd = item;
                itemToAdd.IsLocallyAvailable = false;

                var localOption = localOptionsResult.FirstOrDefault(x => x.OptionId == item.Id);

                if (localOption != null)
                {
                    itemToAdd.IsLocallyAvailable = localOption.IsActive;
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
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var globalOptionResult = _optionsRepository.AsQueryable().Where(x => x.Id == key);

            if (!globalOptionResult.Any())
                return NotFound();

            var option = globalOptionResult.First();

            var localOptionResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == key);

            if (localOptionResult.Any())
            {
                var localOption = localOptionResult.First();
                option.IsLocallyAvailable = localOption.IsActive;
                if (!IsNullOrEmpty(localOption.Description))
                {
                    option.Description = localOption.Description;
                }
            }
            else
            {
                option.IsLocallyAvailable = false;
            }

            return Ok(option);
        }

        public override IHttpActionResult Post(TLocalModelType entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity.OrganizationId = _authService.GetCurrentOrganizationId(UserId);

            if (!_authService.HasWriteAccess(UserId, entity))
            {
                return Forbidden();
            }

            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOptionSearch = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == entity.OptionId);

            if(localOptionSearch.Any())
            {
                try
                {
                    var localOption = localOptionSearch.First();

                    localOption.IsActive = true;
                    Repository.Save();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            } else
            {
                try
                {
                    entity.ObjectOwnerId = UserId;
                    entity.LastChangedByUserId = UserId;
                    entity.IsActive = true;
                    var entityWithOrganization = entity as IHasOrganization;
                    if (entityWithOrganization != null)
                    {
                        entityWithOrganization.OrganizationId = _authService.GetCurrentOrganizationId(UserId);
                    }
                    entity = Repository.Insert(entity);
                    Repository.Save();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }

            return Ok();
        }

        public override IHttpActionResult Patch(int key, Delta<TLocalModelType> delta)
        {
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOptionSearch = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == key);

            if (localOptionSearch.Any())
            {
                var localOption = localOptionSearch.First();
                // does the entity exist?
                if (localOption == null)
                {
                    return NotFound();
                }

                // check if user is allowed to write to the entity
                if (!_authService.HasWriteAccess(UserId, localOption))
                {
                    return Forbidden();
                }

                // check model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

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
            }
            else {
                try
                {
                    TLocalModelType entity = new TLocalModelType();
                    entity.ObjectOwnerId = UserId;
                    entity.LastChangedByUserId = UserId;
                    entity.OptionId = key;
                    var entityWithOrganization = entity as IHasOrganization;
                    if (entityWithOrganization != null)
                    {
                        entityWithOrganization.OrganizationId = _authService.GetCurrentOrganizationId(UserId);
                    }
                    delta.Patch(entity);
                    entity = Repository.Insert(entity);
                    Repository.Save();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }

            return Ok();
        }

        public override IHttpActionResult Delete(int key)
        {
            var orgId = _authService.GetCurrentOrganizationId(UserId);
            LocalOptionEntity<TOptionType> localOption = Repository.AsQueryable().First(x => x.OrganizationId == orgId && x.OptionId == key);

            if (localOption == null)
                return NotFound();

            if (!_authService.HasWriteAccess(UserId, localOption))
            {
                return Forbidden();
            }

            try
            {
                //Repository.DeleteByKey(localOption.Id);

                localOption.IsActive = false;
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
