using System;
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
    public abstract class GenericHasRightsController<TObject, TRight, TRole, TDto> : GenericApiController<TObject, TDto>
        where TObject : Entity, IHasRights<TRight>
        where TRight : Entity, IRight<TObject, TRight, TRole>
        where TRole : IRoleEntity<TRight>
    {
        private readonly IGenericRepository<TRight> _rightRepository;

        protected GenericHasRightsController(IGenericRepository<TObject> repository, IGenericRepository<TRight> rightRepository ) : base(repository)
        {
            _rightRepository = rightRepository;
        }

        /// <summary>
        /// Returns all rights for an object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>List of all rights</returns>
        protected virtual IEnumerable<TRight> GetAllRightsQuery(int id)
        {
            return _rightRepository.Get(right => right.ObjectId == id);
        }

        protected override bool HasWriteAccess(TObject obj, User user)
        {
            //Check for rights on the object
            var rights = GetAllRightsQuery(obj.Id).Where(right => right.UserId == user.Id);
            if(rights.Any(right => right.Role.HasWriteAccess)) return true;

            return base.HasWriteAccess(obj, user);
        }

        /// <summary>
        /// Get all rights for a given object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <param name="rights">Routing qualifier</param>
        /// <returns>List of rights</returns>
        public HttpResponseMessage GetRights(int id, bool? rights)
        {
            try
            {
                var theRights = GetAllRightsQuery(id);
                var dtos = Map<IEnumerable<TRight>, IEnumerable<RightOutputDTO>>(theRights);

                return Ok(dtos);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// Post a new right to the object
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public HttpResponseMessage PostRight(int id, RightInputDTO dto)
        {
            try
            {
                if (!HasWriteAccess(id, KitosUser))
                    return Unauthorized();

                var right = AutoMapper.Mapper.Map<RightInputDTO, TRight>(dto);
                right.ObjectId = id;

                right = _rightRepository.Insert(right);
                _rightRepository.Save();

                //TODO: FIX navigation properties not loading properly!!!
                right.User = UserRepository.GetByKey(right.UserId);

                var outputDTO = AutoMapper.Mapper.Map<TRight, RightOutputDTO>(right);

                return Created(outputDTO);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

    }



    //public abstract class GenericRightController<TRight, TObject, TRole> : GenericApiController<TRight, RightInputDTO, RightOutputDTO> 
    //    where TRight : Entity, IRight
    //    where TObject : Entity
    //    where TRole : Entity, IRoleEntity
    //{
    //    protected readonly IGenericRepository<TRight> RightRepository;
    //    private readonly IGenericRepository<TObject> _objectRepository;

    //    protected GenericRightController(IGenericRepository<TRight> rightRepository, IGenericRepository<TObject> objectRepository)
    //        : base(rightRepository)
    //    {
    //        RightRepository = rightRepository;
    //        _objectRepository = objectRepository;
    //    }

    //    protected virtual IEnumerable<TRight> GetAll(int oId)
    //    {
    //        return RightRepository.Get(right => right.ObjectId == oId);
    //    } 

    //    protected virtual bool HasWriteAccess(TObject theObject, User user)
    //    {
    //        if (theObject.ObjectOwnerId == user.Id) return true; 

    //        var rights = RightRepository.Get(right => right.ObjectId == theObject.Id && right.UserId == user.Id).ToList();
    //        return rights.Any(right => right.Role.HasWriteAccess);
    //    }

    //    private bool HasWriteAccess(int objId, int userId)
    //    {
    //        var user = UserRepository.GetByKey(userId);
    //        var theObject = _objectRepository.GetByKey(objId);
    //        return HasWriteAccess(theObject, user);
    //    }

    //    public HttpResponseMessage Get(int id)
    //    {
    //        try
    //        {
    //            var rights = GetAll(id);
    //            var dtos = AutoMapper.Mapper.Map<IEnumerable<TRight>, IEnumerable<RightOutputDTO>>(rights);

    //            return Ok(dtos);
    //        }
    //        catch (Exception e)
    //        {
    //            return Error(e);
    //        }
    //    }

    //    public HttpResponseMessage GetHasWriteAccess(int id, bool? hasWriteAccess)
    //    {
    //        try
    //        {
    //            return Ok(HasWriteAccess(id, KitosUser.Id));
    //        }
    //        catch (Exception e)
    //        {
    //            return Error(e);
    //        }
    //    }

    //    public HttpResponseMessage GetHasWriteAccess(bool? hasWriteAccess, int oId, int uId)
    //    {
    //        try
    //        {
    //            return Ok(HasWriteAccess(oId, uId));
    //        }
    //        catch (Exception e)
    //        {
    //            return Error(e);
    //        }
    //    }

    //    public HttpResponseMessage Post(RightInputDTO inputDTO)
    //    {
    //        try
    //        {
    //            if (!HasWriteAccess(inputDTO.ObjectId, KitosUser.Id))
    //                return Unauthorized();

    //            var right = AutoMapper.Mapper.Map<RightInputDTO, TRight>(inputDTO);

    //            right = RightRepository.Insert(right);
    //            RightRepository.Save();

    //            //TODO: FIX navigation properties not loading properly!!!
    //            right.User = UserRepository.GetByKey(right.UserId);

    //            var outputDTO = AutoMapper.Mapper.Map<TRight, RightOutputDTO>(right);

    //            return Created(outputDTO);
    //        }
    //        catch (Exception e)
    //        {
    //            return Error(e);
    //        }
    //    }

    //    public HttpResponseMessage Delete([FromUri] int oId, [FromUri] int rId, [FromUri] int uId)
    //    {
    //        try
    //        {
    //            RightRepository.DeleteByKey(oId, rId, uId);
    //            RightRepository.Save();

    //            return Ok();
    //        }
    //        catch (Exception e)
    //        {
    //            return Error(e);
    //        }
    //    }

    //}

}
