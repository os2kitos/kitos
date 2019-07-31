using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Core.ApplicationServices;

namespace Presentation.Web.Controllers.API
{
    public class ReferenceController : GenericApiController<ExternalReference, ExternalReferenceDTO>
    {
        public readonly IFeatureChecker _featureChecker;
        public ReferenceController(IGenericRepository<ExternalReference> repository, IFeatureChecker featureChecker) : base(repository)
        {
            _featureChecker = featureChecker;
        }

        public override HttpResponseMessage Patch(int id, int organizationId, JObject obj)
        {
            var reference = Repository.GetByKey(id);
            if (!CanModifyReference(reference))
            {
                return Forbidden();
            }

            var result = base.PatchQuery(reference, obj);
            {
                return Ok(Map(result));
            }
        }

        private bool CanModifyReference(ExternalReference entity)
        {
            if (entity.ObjectOwnerId == KitosUser.Id)
            {
                return true;
            }

            if (_featureChecker.CanExecute(KitosUser, Feature.CanModifyContracts) && entity.ItContract != null)
            {
                return true;
            }

            if (_featureChecker.CanExecute(KitosUser, Feature.CanModifyProjects) && entity.ItProject != null)
            {
                return true;
            }

            if (_featureChecker.CanExecute(KitosUser, Feature.CanModifySystems) && entity.ItSystem != null)
            {
                return true;
            }

            if (_featureChecker.CanExecute(KitosUser, Feature.CanModifySystems) && entity.ItSystemUsage != null)
            {
                return true;
            }

            return false;
        }

        /*  public override HttpResponseMessage Post(ExternalReferenceDTO dto)
          {
              dto.ItProjectId
              return base.Post(dto);
          }*/
    }
}