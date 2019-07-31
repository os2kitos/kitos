using System.Linq;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;
using System.Net.Http;
using Core.DomainModel;
using System;

namespace Presentation.Web.Controllers.API
{
    public class EconomyStreamController : GenericContextAwareApiController<EconomyStream, EconomyStreamDTO>
    {
        private readonly IGenericRepository<ItContract> _contracts;

        public EconomyStreamController(IGenericRepository<EconomyStream> repository, IGenericRepository<ItContract> contracts) : base(repository)
        {
            this._contracts = contracts;
        }

        public HttpResponseMessage GetExternEconomyStreamForContract(int externPaymentForContractWithId)
        {
            var result = Repository.AsQueryable().Where(e => e.ExternPaymentForId == externPaymentForContractWithId);
            var currentOrgId = KitosUser.DefaultOrganizationId;

            if (AuthenticationService.HasReadAccessOutsideContext(KitosUser.Id))
            {
                if (!AuthenticationService.IsGlobalAdmin(KitosUser.Id) && result.Any())
                {
                    // all users may view economy streams marked Public or if they are part of the organization
                    result = result.Where(x => x.AccessModifier == AccessModifier.Public || x.ExternPaymentFor.OrganizationId == currentOrgId);
                    if (!result.Any())
                        //at this point the economy streams are marked Local but the user is not part of the organization which means they are not authorized to view the data
                        return Forbidden();
                }
            }
            else
            {
                result = result.Where(x => x.OrganizationUnit.OrganizationId == currentOrgId);
            }

            return Ok(Map(result));
        }

        public HttpResponseMessage GetInternEconomyStreamForContract(int internPaymentForContractWithId)
        {
            var result = Repository.AsQueryable().Where(e => e.InternPaymentForId == internPaymentForContractWithId);
            var currentOrgId = KitosUser.DefaultOrganizationId;

            if (AuthenticationService.HasReadAccessOutsideContext(KitosUser.Id))
            {
                if (!AuthenticationService.IsGlobalAdmin(KitosUser.Id) && result.Any())
                {
                    // all users may view economy streams marked Public or if they are part of the organization
                    result = result.Where(x => x.AccessModifier == AccessModifier.Public || x.InternPaymentFor.OrganizationId == currentOrgId);
                    if (!result.Any())
                        //at this point the economy streams are marked Local but the user is not part of the organization which means they are not authorized to view the data
                    {
                        return Forbidden();
                    }
                }
            }
            else
            {
                result = result.Where(x => x.OrganizationUnit.OrganizationId == currentOrgId);
            }

            return Ok(Map(result));
        }

        public HttpResponseMessage Post(int contractId, EconomyStreamDTO streamDTO)
        {
            var contract = _contracts.GetByKey(contractId);

            if (contract == null)
            {
                return NotFound();
            }

            var stream = Map<EconomyStreamDTO, EconomyStream>(streamDTO);

            if (streamDTO.ExternPaymentForId != null)
            {
                stream.ExternPaymentFor = contract;
            }
            else
            {
                stream.InternPaymentFor = contract;
            }

            if (!AuthenticationService.HasWriteAccess(KitosUser.Id, stream))
            {
                return Forbidden();
            }

            stream.ObjectOwner = KitosUser;
            stream.LastChangedByUser = KitosUser;

            var savedItem = PostQuery(stream);

            return Created(Map(savedItem), new Uri(Request.RequestUri + "/" + savedItem.Id));
        }
    }
}
