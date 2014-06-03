using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models.Exceptions;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericApiController<TModel, TDto> : BaseApiController
        where TModel : Entity
    {
        protected readonly IGenericRepository<TModel> Repository;

        protected GenericApiController(IGenericRepository<TModel> repository)
        {
            Repository = repository;
        }
        
        protected virtual IEnumerable<TModel> GetAllQuery()
        {
            //TODO: remove this hardcode and do some proper paging
            return Repository.Get().Take(100);
        }

        public virtual HttpResponseMessage GetAll()
        {
            try
            {
                var items = GetAllQuery();

                return Ok(Map(items));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        // GET api/T
        public virtual HttpResponseMessage GetSingle(int id)
        {
            try
            {
                var item = Repository.GetByKey(id);

                if (item == null) return NotFound();

                return Ok(Map(item));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        /// <summary>
        /// GET api/T/id?hasWriteAccess
        /// Returns whether the current authenticated user has write access 
        /// to the object with the given id
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <param name="hasWriteAccess">Route qualifier</param>
        /// <returns>True or false</returns>
        public HttpResponseMessage GetHasWriteAccess(int id, bool? hasWriteAccess)
        {
            try
            {
                return Ok(HasWriteAccess(id));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected virtual TModel PostQuery(TModel item)
        {
            Repository.Insert(item);
            Repository.Save();

            return item; // TODO this doesn't need to return
        }

        // POST api/T
        public virtual HttpResponseMessage Post(TDto dto)
        {
            try
            {
                var item = Map<TDto, TModel>(dto);

                item.ObjectOwner = KitosUser;

                PostQuery(item);

                return Created(Map(item), new Uri(Request.RequestUri + "/" + item.Id));
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected virtual TModel PutQuery(TModel item)
        {
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        // PUT api/T
        public virtual HttpResponseMessage Put(int id, TDto dto)
        {
            try
            {
                var oldItem = Repository.GetByKey(id);
                if (!HasWriteAccess(oldItem)) return Unauthorized();

                var newItem = Map(dto);
                newItem.Id = id;

                PutQuery(newItem);

                return Ok();
            }
            catch (Exception)
            {
                return NoContent();
            }
        }

        protected virtual void DeleteQuery(int id)
        {
            Repository.DeleteByKey(id);
            Repository.Save();
        }

        // DELETE api/T
        public virtual HttpResponseMessage Delete(int id)
        {
            try
            {
                var item = Repository.GetByKey(id);
                if (!HasWriteAccess(item)) return Unauthorized();

                DeleteQuery(id);

                return Ok();
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected virtual TModel PatchQuery(TModel item)
        {
            Repository.Update(item);
            Repository.Save();

            return item;
        }

        // PATCH api/T
        public virtual HttpResponseMessage Patch(int id, JObject obj)
        {
            try
            {
                var item = Repository.GetByKey(id);
                if (!HasWriteAccess(item)) return Unauthorized();

                var itemType = item.GetType();

                foreach (var valuePair in obj)
                {
                    // get name of mapped property
                    var map =
                        AutoMapper.Mapper.FindTypeMapFor<TDto, TModel>()
                                  .GetPropertyMaps();
                    var nonNullMaps = map.Where(x => x.SourceMember != null);
                    var mapMember = nonNullMaps.SingleOrDefault(x => x.SourceMember.Name.Equals(valuePair.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (mapMember == null)
                        continue; // abort if no map found

                    var destName = mapMember.DestinationProperty.Name;
                    var jToken = valuePair.Value;

                    var propRef = itemType.GetProperty(destName);
                    var t = propRef.PropertyType;

                    // we have to handle enums separately
                    if (t.IsEnum)
                    {
                        var value = valuePair.Value.Value<int>();
                        propRef.SetValue(item, value);
                    }
                    else
                    {
                        try
                        {
                            // get reference to the generic method obj.Value<t>(parameter);
                            var genericMethod = jToken.GetType().GetMethod("Value").MakeGenericMethod(new Type[] { t });
                            // use reflection to call obj.Value<t>("keyName");
                            var value = genericMethod.Invoke(obj, new object[] { valuePair.Key });
                            // update the entity
                            propRef.SetValue(item, value);
                        }
                        catch (Exception)
                        {
                            // if obj.Value<t>("keyName") cast fails set to fallback value
                            propRef.SetValue(item, null); // TODO this is could be dangerous, should probably also be default(t)
                        }
                    }
                }
                
                PatchQuery(item);
                return Ok(Map(item));
            }
            catch (Exception e)
            {
                return Error(e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Repository.Dispose();
            base.Dispose(disposing);
        }

        #region Write Access Checks functions

        /// <summary>
        /// Checks if a given user has write access to a given object. 
        /// Override this method as needed.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="user">The user</param>
        /// <returns>True iff user has write access to obj</returns>
        protected bool HasWriteAccess(TModel obj, User user)
        {
            return obj.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Checks if the current authenticated user has write access to a given object. 
        /// </summary>
        /// <param name="objId">The id of object</param>
        /// <returns>True iff user has write access to the object with objId</returns>
        protected bool HasWriteAccess(int objId)
        {
            return HasWriteAccess(objId, KitosUser);
        }

        /// <summary>
        /// Checks if a given user has write access to a given object. 
        /// </summary>
        /// <param name="objId">The id of object</param>
        /// <param name="user">The user</param>
        /// <returns>True iff user has write access to the object with objId</returns>
        protected bool HasWriteAccess(int objId, User user)
        {
            var obj = Repository.GetByKey(objId);
            return HasWriteAccess(obj, user);
        }

        /// <summary>
        /// Checks if the current authenticated user has write access to a given object. 
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>True iff user has write access to obj</returns>
        protected bool HasWriteAccess(TModel obj)
        {
            return HasWriteAccess(obj, KitosUser);
        }

        #endregion

        #region Mapping functions

        //for easy access
        protected virtual TDto Map(TModel model)
        {
            return Map<TModel, TDto>(model);
        }

        //for easy access
        protected virtual TModel Map(TDto inputDto)
        {
            return Map<TDto, TModel>(inputDto);
        }

        //for easy access (list)
        protected virtual IEnumerable<TDto> Map(IEnumerable<TModel> models)
        {
            return Map<IEnumerable<TModel>, IEnumerable<TDto>>(models);
        }

        //for easy access (list)
        protected virtual IEnumerable<TModel> Map(IEnumerable<TDto> inputDtos)
        {
            return Map<IEnumerable<TDto>, IEnumerable<TModel>>(inputDtos);
        }

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return AutoMapper.Mapper.Map<TDest>(item);
        }

        #endregion

    }
}
