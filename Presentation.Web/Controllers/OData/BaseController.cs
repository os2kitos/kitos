using System;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Ninject;

namespace Presentation.Web.Controllers.OData
{
    //[Authorize]
    public abstract class BaseController<T> : ODataController where T : class 
    {
        protected ODataValidationSettings ValidationSettings;
        protected IGenericRepository<T> Repository;
        private User _curentUser;

        [Inject]
        public IGenericRepository<User> UserRepository { get; set; }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

        protected BaseController(IGenericRepository<T> repository)
        {
            ValidationSettings = new ODataValidationSettings { AllowedQueryOptions = AllowedQueryOptions.All };
            Repository = repository;
        }


        public User CurentUser
        {
            get
            {
                if (_curentUser == null && UserId != 0)
                    _curentUser = UserRepository.GetByKey(UserId);

                return _curentUser;
            }
            set { _curentUser = value; }
        }

        protected int UserId
        {
            get
            {
                int userId;
                int.TryParse(User.Identity.Name, out userId);
                return userId;
            }
        }

        [EnableQuery]
        public virtual IHttpActionResult Get()
        {
            return Ok(Repository.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 4)]
        public virtual IHttpActionResult Get(int key)
        {
            var entity = Repository.GetByKey(key);
            return Ok(entity);
        }

       
        // TODO for now only read actions are allowed, in future write will be enabled - but keep security in mind!

        //protected IHttpActionResult Put(int key, T entity)
        //{
        //    return StatusCode(HttpStatusCode.NotImplemented);
        //}

        //public virtual IHttpActionResult Post(T entity)
        //{
        //    if (!AuthenticationService.HasWriteAccess(CurentUser, entity))
        //        return Unauthorized();

        //    Validate(entity);

        //    T newEntity;

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    try
        //    {
        //        var e = entity as Entity;
        //        if (e != null)
        //        {
        //            e.ObjectOwner = CurentUser;
        //            e.LastChangedByUser = CurentUser;
        //        }
        //        newEntity = Repository.Insert(entity);
        //        Repository.Save();
        //    }
        //    catch (Exception e)
        //    {
        //        return InternalServerError(e);
        //    }

        //    return Created(newEntity);
        //}

        //protected IHttpActionResult Patch(int key, Delta<T> delta)
        //{
        //    Validate(delta.GetEntity());

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var entity = Repository.GetByKey(key);
        //    if (entity == null)
        //        return NotFound();

        //    if (!AuthenticationService.HasWriteAccess(UserId, entity))
        //        return Unauthorized();

        //    try
        //    {
        //        delta.Patch(entity);
        //        Repository.Save();
        //    }
        //    catch (Exception e)
        //    {
        //        return InternalServerError(e);
        //    }
        //    return Updated(entity);
        //}

        //protected IHttpActionResult Delete(int key)
        //{
        //    var entity = Repository.GetByKey(key);
        //    if (entity == null)
        //        return NotFound();

        //    try
        //    {
        //        Repository.DeleteByKey(key);
        //        Repository.Save();
        //    }
        //    catch (Exception e)
        //    {
        //        return InternalServerError(e);
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}
    }
}
