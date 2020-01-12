using Core.DomainModel;
using Core.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Core.DomainServices.Authorization;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.AttachedOptions
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class AttachedOptionsFunctionController<TEntity, TOption, TLocalOption> : AttachedOptionsController
    where TEntity : Entity
    where TOption : OptionHasChecked<TEntity>
    where TLocalOption : LocalOptionEntity<TOption>
    {
        private readonly IGenericRepository<AttachedOption> _attachedOptionRepository;
        private readonly IGenericRepository<TOption> _optionRepository;
        private readonly IGenericRepository<TLocalOption> _localOptionRepository;

        public AttachedOptionsFunctionController(
            IGenericRepository<AttachedOption> repository, 
            IGenericRepository<TOption> optionRepository,
            IGenericRepository<TLocalOption> localOptionRepository)
               : base(repository)
        {
            _attachedOptionRepository = repository;
            _optionRepository = optionRepository;
            _localOptionRepository = localOptionRepository;
        }

        public virtual IHttpActionResult GetOptionsByObjectIDAndType(int id, EntityType entitytype, OptionType optiontype)
        {
            var orgId = AuthService.GetCurrentOrganizationId(UserId);

            var globalOptionData = _optionRepository.AsQueryable().Where(s => s.IsEnabled);
            var localpersonalData = _localOptionRepository.AsQueryable().Where(p => p.IsActive && p.OrganizationId == orgId).ToList();

            var result = new List<TOption>();
            result.AddRange(globalOptionData.AsQueryable().Where(s => s.IsObligatory));

            foreach (var p in localpersonalData)
            {
                var data = globalOptionData.AsQueryable().FirstOrDefault(s => s.Id == p.OptionId && (s.IsEnabled && !s.IsObligatory));
                if (data != null)
                {
                    result.Add(data);
                }
            }

            return CreateOptionsResponse(id, entitytype, optiontype, result);
        }

        [HttpDelete]
        [EnableQuery]
        [ODataRoute("RemoveOption(id={id}, objectId={objectId}, type={type}, entityType={entityType})")]
        public IHttpActionResult RemoveOption(int id, int objectId, OptionType type, EntityType entityType)
        {
            var option = _attachedOptionRepository.AsQueryable().FirstOrDefault(o => o.OptionId == id 
            && o.ObjectId == objectId 
            && o.OptionType == type
            && o.ObjectType == entityType);

            if (option == null)
            {
                return NotFound();
            }

            if (!AllowWrite(option))
            {
                return Forbidden();
            }

            try
            {
                Repository.DeleteByKey(option.Id);
                Repository.Save();
            }
            catch (Exception e)
            {
                return InternalServerError(e);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected IHttpActionResult CreateOptionsResponse(int id, EntityType entitytype, OptionType optiontype, List<TOption> result)
        {
            var options = GetAttachedOptions(optiontype, id, entitytype);

            if (options != null)
            {
                if (!options.Any())
                {
                    return Ok(result);
                }

                foreach (var o in options)
                {
                    var currentOption = result.FirstOrDefault(r => r.Id == o.OptionId);
                    if (currentOption != null)
                    {
                        currentOption.Checked = true;
                    }
                    else
                    {
                        _attachedOptionRepository.Delete(o);
                        _attachedOptionRepository.Save();
                    }
                }
            }
            else
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return Ok(result);
        }

        protected List<AttachedOption> GetAttachedOptions(OptionType type, int id, EntityType objectType)
        {
            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(AttachedOption));

            if (GetCrossOrganizationReadAccessLevel() == CrossOrganizationDataReadAccessLevel.All || hasOrg == false)
            {
                //tolist so we can operate with open datareaders in the following foreach loop.
                return _attachedOptionRepository.AsQueryable().Where(x => x.ObjectId == id
                && x.OptionType == type
                && x.ObjectType == objectType).ToList();
            }

            return _attachedOptionRepository.AsQueryable()
                .Where(x => ((IHasOrganization)x).OrganizationId == AuthService.GetCurrentOrganizationId(UserId)
                            && x.ObjectId == id
                            && x.OptionType == type
                            && x.ObjectType == objectType).ToList();
        }
    }
}