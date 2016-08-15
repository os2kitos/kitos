using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    [Authorize]
    public class EconomyStreamsController : ODataController // doesn't derive from BaseEntityController because we need absolute control over what is exposed here
    {
        private readonly IGenericRepository<EconomyStream> _repository;
        private readonly IGenericRepository<User> _userRepository;

        public EconomyStreamsController(IGenericRepository<EconomyStream> repository, IGenericRepository<User> userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // GET /Organizations(1)/ItContracts
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("ExternEconomyStreams(Organization={orgKey})")]
        public IHttpActionResult GetByOrganization(int orgKey)
        {
            if (!HasAccessWithinOrganization(orgKey))
                return Unauthorized();

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.ExternPaymentFor.OrganizationId == orgKey &&
                            m.InternPaymentFor == null);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)/ExternEconomyStreams
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})/ExternEconomyStreams")]
        public IHttpActionResult GetAllExtern(int orgKey, int contractKey)
        {
            if (!HasAccessWithinOrganization(orgKey))
                return Unauthorized();

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.ExternPaymentFor.OrganizationId == orgKey && m.ExternPaymentForId == contractKey &&
                            m.InternPaymentFor == null);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)/InternEconomyStreams
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})/InternEconomyStreams")]
        public IHttpActionResult GetAllIntern(int orgKey, int contractKey)
        {
            if (!HasAccessWithinOrganization(orgKey))
                return Unauthorized();

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.ExternPaymentForId == null && m.InternPaymentForId == contractKey &&
                            m.InternPaymentFor.OrganizationId == orgKey);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)/ExternEconomyStreams(1)
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})/ExternEconomyStreams({key})")]
        public IHttpActionResult GetSingleExtern(int orgKey, int contractKey, int key)
        {
            if (!HasAccessWithinOrganization(orgKey))
                return Unauthorized();

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.Id == key &&
                            m.ExternPaymentFor.OrganizationId == orgKey && m.ExternPaymentForId == contractKey &&
                            m.InternPaymentFor == null);
            return Ok(result);
        }

        // GET /Organizations(1)/ItContracts(1)/InternEconomyStreams(1)
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All & ~AllowedQueryOptions.Expand)]
        [ODataRoute("Organizations({orgKey})/ItContracts({contractKey})/InternEconomyStreams({key})")]
        public IHttpActionResult GetSingleIntern(int orgKey, int contractKey, int key)
        {
            if (!HasAccessWithinOrganization(orgKey))
                return Unauthorized();

            var result =
                _repository.AsQueryable()
                    .Where(
                        m =>
                            m.Id == key && m.ExternPaymentForId == null && m.InternPaymentForId == contractKey &&
                            m.InternPaymentFor.OrganizationId == orgKey);
            return Ok(result);
        }

        private bool HasAccessWithinOrganization(int orgKey)
        {
            var id = Convert.ToUInt32(User.Identity.Name);
            var user = _userRepository.Get(u => u.Id == id).FirstOrDefault();
            var hasRightsOnOrganization = user != null && user.OrganizationRights.Any(x => x.OrganizationId == orgKey);
            return hasRightsOnOrganization;
        }
    }
}
