﻿using Core.DomainModel;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Core.DomainServices.Extensions;
using Microsoft.AspNet.OData;
using static System.String;

namespace Presentation.Web.Controllers.OData
{
    public abstract class LocalOptionBaseController<TLocalModelType, TDomainModelType, TOptionType> : BaseEntityController<TLocalModelType> where TLocalModelType : LocalOptionEntity<TOptionType>, new() where TOptionType : OptionEntity<TDomainModelType>
    {
        private readonly IGenericRepository<TOptionType> _optionsRepository;

        protected LocalOptionBaseController(IGenericRepository<TLocalModelType> repository, IGenericRepository<TOptionType> optionsRepository)
            : base(repository)
        {
            _optionsRepository = optionsRepository;
        }

        [NonAction]
        public override IHttpActionResult Get() => throw new NotSupportedException();

        public virtual IHttpActionResult GetByOrganizationId(int organizationId)
        {
            var localOptionsResult = Repository.AsQueryable().ByOrganizationId(organizationId).ToList();
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

        [NonAction]
        public override IHttpActionResult Get(int key) => throw new NotSupportedException();

        public virtual IHttpActionResult Get(int organizationId, int key)
        {
            //TODO-MRJ_FRONTEND: Update front-end
            var globalOptionResult = _optionsRepository.AsQueryable().Where(x => x.Id == key);

            if (!globalOptionResult.Any())
                return NotFound();

            var option = globalOptionResult.First();

            var localOptionResult =
                Repository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .Where(x => x.OptionId == key);

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

        public override IHttpActionResult Post(int organizationId, TLocalModelType entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO-MRJ_FRONTEND: Update frontend (perhaps patch using an interceptor)?

            entity.OrganizationId = organizationId;

            if (!AllowCreate<TLocalModelType>(organizationId, entity))
            {
                return Forbidden();
            }

            var localOptionSearch = Repository.AsQueryable().Where(x => x.OrganizationId == organizationId && x.OptionId == entity.OptionId);

            if (localOptionSearch.Any())
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
            }
            else
            {
                try
                {
                    entity.IsActive = true;

                    if (entity is IOwnedByOrganization entityWithOrganization)
                    {
                        entityWithOrganization.OrganizationId = organizationId;
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

        [NonAction]
        public override IHttpActionResult Patch(int key, Delta<TLocalModelType> delta) => throw new NotSupportedException();

        public virtual IHttpActionResult Patch(int organizationId, int key, Delta<TLocalModelType> delta)
        {
            //TODO-MRJ_FRONTEND: Update front-end
            var orgId = organizationId;
            var localOptionSearch =
                Repository
                    .AsQueryable()
                    .ByOrganizationId(organizationId)
                    .Where(x => x.OptionId == key);

            if (localOptionSearch.Any())
            {
                var localOption = localOptionSearch.First();
                // does the entity exist?
                if (localOption == null)
                {
                    return NotFound();
                }

                // check if user is allowed to write to the entity
                if (!AllowModify(localOption))
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
            else
            {
                try
                {
                    TLocalModelType entity = new TLocalModelType();
                    entity.OptionId = key;

                    if (entity is IOwnedByOrganization entityWithOrganization)
                    {
                        entityWithOrganization.OrganizationId = orgId;
                    }

                    delta.Patch(entity);
                    Repository.Insert(entity);
                    Repository.Save();
                }
                catch (Exception e)
                {
                    return InternalServerError(e);
                }
            }

            return Ok();
        }

        [NonAction] //TODO-MRJ: Consider if we could just use the actual id in stead of optionid?
        public override IHttpActionResult Delete(int key) => throw new NotSupportedException();

        public virtual IHttpActionResult Delete(int organizationId, int key)
        {
            //TODO-MRJ_FRONTEND: Update front-end
            var orgId = organizationId;
            LocalOptionEntity<TOptionType> localOption = Repository.AsQueryable().First(x => x.OrganizationId == orgId && x.OptionId == key);

            if (localOption == null)
                return NotFound();

            if (!AllowDelete(localOption))
            {
                return Forbidden();
            }

            try
            {
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
