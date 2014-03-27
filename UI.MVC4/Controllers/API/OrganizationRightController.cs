using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationRightController : BaseApiController
    {
        private readonly IGenericRepository<OrganizationRight> _repository;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRightController(IGenericRepository<OrganizationRight> repository, IOrgUnitService orgUnitService)
        {
            _repository = repository;
            _orgUnitService = orgUnitService;
        }

        /* returns the organisations-rights for an organization unit and all units in the subtree */
        public HttpResponseMessage GetSubTreeRights(int organizationUnitId)
        {
            try
            {
                var orgUnits = _orgUnitService.GetSubTree(organizationUnitId);

                var rights = new List<OrganizationRight>();
                foreach (var orgUnit in orgUnits)
                {
                    var unit = orgUnit;
                    rights.AddRange(_repository.Get(right => right.Object_Id == unit.Id));
                }

                var dtos = AutoMapper.Mapper.Map<ICollection<OrganizationRight>, ICollection<RightOutputDTO>>(rights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /* TODO: this is not very RESTful (just look at the verbs 'get-has') */
        //returns true if the user with userId has writeAccess to the orgUnit with organizationUnitId
        public HttpResponseMessage GetHasWriteAccess(bool? hasWriteAccess, int orgUnitId, int userId)
        {
            try
            {
                var user = UserRepository.GetByKey(userId);

                var result = _orgUnitService.HasWriteAccess(user, orgUnitId);

                return Ok(result);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Post(RightInputDTO inputDTO)
        {
            try
            {
                if (!_orgUnitService.HasWriteAccess(KitosUser, inputDTO.Object_Id))
                    throw new SecurityException("User doesn't have write permission for that Organization Unit");

                var right = AutoMapper.Mapper.Map<RightInputDTO, OrganizationRight>(inputDTO);

                right = _repository.Insert(right);
                _repository.Save();
                //TODO: FIX navigation properties not loading properly!!!
                var outputDTO = AutoMapper.Mapper.Map<OrganizationRight, RightOutputDTO>(right);

                return Created(outputDTO);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        public HttpResponseMessage Delete([FromUri] int oId, [FromUri] int rId, [FromUri] int uId)
        {
            try
            {
                _repository.DeleteByKey(oId, rId, uId);
                _repository.Save();

                return Ok();
            }
            catch (Exception)
            {
                return NoContent(); // TODO catch correct expection
            }
        }
        
        /*
        public HttpResponseMessage Put([FromUri] int oId, [FromUri] int rId, [FromUri] int uId, [FromBody] RightInputDTO inputDTO)
        {
            try
            {
                if (!_orgUnitService.HasWriteAccess(KitosUser, inputDTO.Object_Id))
                    throw new SecurityException("User doesn't have write permission for that Organization Unit");

                _repository.DeleteByKey(oId, rId, uId);

                right.Object_Id = inputDTO.Object_Id;
                right.Role_Id = inputDTO.Role_Id;
                right.User_Id = inputDTO.User_Id;

                _repository.Update(right);
                _repository.Save();

                //TODO: FIX navigation properties not loading properly!!!

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }*/
    }
}
