using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemRightController : BaseApiController
    {
        private readonly IGenericRepository<ItSystemRight> _repository;

        public ItSystemRightController(IGenericRepository<ItSystemRight> repository)
        {
            _repository = repository;
        }

        // TODO: this is copied from org unit controller and should be modified to work with it systems
        //returns true if the user with userId has writeAccess to the orgUnit with organizationUnitId
        //public HttpResponseMessage GetHasWriteAccess(bool? hasWriteAccess, int orgUnitId, int userId)
        //{
        //    try
        //    {
        //        var user = UserRepository.GetByKey(userId);

        //        var result = _orgUnitService.HasWriteAccess(user, orgUnitId);

        //        return Ok(result);
        //    }
        //    catch (Exception e)
        //    {
        //        return Error(e);
        //    }
        //}

        public HttpResponseMessage Get([FromUri] int usageId)
        {
            try
            {
                var rights = _repository.Get(u => u.ObjectId == usageId);

                var dtos = AutoMapper.Mapper.Map<IEnumerable<ItSystemRight>, IEnumerable<RightOutputDTO>>(rights);

                return Ok(dtos);
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
                /*if (!_orgUnitService.HasWriteAccess(KitosUser, inputDTO.ObjectId))
                    throw new SecurityException("User doesn't have write permission for that Organization Unit");*/

                var right = AutoMapper.Mapper.Map<RightInputDTO, ItSystemRight>(inputDTO);

                right = _repository.Insert(right);
                _repository.Save(); 

                right.User = UserRepository.GetByKey(right.UserId);

                //TODO: FIX navigation properties not loading properly!!!
                var outputDTO = AutoMapper.Mapper.Map<ItSystemRight, RightOutputDTO>(right);

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
    }
}
