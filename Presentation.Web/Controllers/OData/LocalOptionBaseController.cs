using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace Presentation.Web.Controllers.OData
{
    public class LocalOptionBaseController<T, Type, OptionType> : BaseEntityController<T> where T : LocalOptionEntity<OptionType> where OptionType : OptionEntity<Type>
    {
        private readonly IAuthenticationService _authService;
        private readonly IGenericRepository<OptionType> _optionsRepository;

        public LocalOptionBaseController(IGenericRepository<T> repository, IAuthenticationService authService, IGenericRepository<OptionType> optionsRepository)
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

            int orgId = _authService.GetCurrentOrganizationId(UserId);
            var localOptionsResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId).ToList();
            var globalOptionsResult = _optionsRepository.AsQueryable().ToList();
            var returnList = new List<OptionType>();

            foreach (var item in globalOptionsResult)
            {
                if (!item.IsEnabled)
                    continue;

                OptionType itemToAdd = item;
                item.IsActive = false;

                var search = localOptionsResult.Where(x => x.OptionId == item.Id);

                if (search.Count() > 0)
                {
                    var localOption = search.First();
                    itemToAdd.IsActive = true;
                    if (!String.IsNullOrEmpty(localOption.Description))
                    {
                        itemToAdd.Description = localOption.Description;
                    }
                }
                returnList.Add(itemToAdd);
            }
            return Ok(returnList);
        }

        [EnableQuery]
        public override IHttpActionResult Get(int globalOptionId)
        {
            if (UserId == 0)
                return Unauthorized();

            int orgId = _authService.GetCurrentOrganizationId(UserId);
            var globalOptionResult = _optionsRepository.AsQueryable().Where(x => x.Id == globalOptionId);

            if (!globalOptionResult.Any())
                return NotFound();

            var option = globalOptionResult.First();

            var localOptionResult = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == globalOptionId);

            if (localOptionResult.Any())
            {
                option.IsActive = true;
                var localOption = localOptionResult.First();
                if (!String.IsNullOrEmpty(localOption.Description))
                {
                    option.Description = localOption.Description;
                }
            } else {
                option.IsActive = false;
            }

            return Ok(option);
        }

        public override IHttpActionResult Delete(int key)
        {
            int orgId = _authService.GetCurrentOrganizationId(UserId);
            LocalOptionEntity<OptionType> localOption = Repository.AsQueryable().Where(x => x.OrganizationId == orgId && x.OptionId == key).First();

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
