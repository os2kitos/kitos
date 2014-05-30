using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Core.DomainModel;
using Core.DomainServices;
using Newtonsoft.Json.Linq;
using UI.MVC4.Models.Exceptions;

namespace UI.MVC4.Controllers.API
{
    public abstract class GenericApiController<TModel, TInputDTO, TOutputDTO> : BaseApiController // TODO perhaps it's possible to infer the TKeyType from TModel somehow
        where TModel : Entity
    {
        protected readonly IGenericRepository<TModel> Repository;

        protected GenericApiController(IGenericRepository<TModel> repository)
        {
            Repository = repository;
        }

        #region mapping functions

        //for easy access
        protected virtual TOutputDTO Map(TModel model)
        {
            return Map<TModel, TOutputDTO>(model);
        }

        //for easy access
        protected virtual TModel Map(TInputDTO inputDto)
        {
            return Map<TInputDTO, TModel>(inputDto);
        }

        //for easy access (list)
        protected virtual IEnumerable<TOutputDTO> Map(IEnumerable<TModel> models)
        {
            return Map<IEnumerable<TModel>, IEnumerable<TOutputDTO>>(models);
        }

        //for easy access (list)
        protected virtual IEnumerable<TModel> Map(IEnumerable<TInputDTO> inputDtos)
        {
            return Map<IEnumerable<TInputDTO>, IEnumerable<TModel>>(inputDtos);
        }

        protected virtual TDest Map<TSource, TDest>(TSource item)
        {
            return AutoMapper.Mapper.Map<TDest>(item);
        }

        #endregion

        protected virtual IEnumerable<TModel> GetAllQuery()
        {
            //TODO: remove this hardcode and do some proper paging
            return Repository.Get().Take(100);
        }

        public HttpResponseMessage GetAll()
        {
            var items = GetAllQuery();

            return Ok(Map<IEnumerable<TModel>, IEnumerable<TOutputDTO>>(items));
        }

        // GET api/T
        public HttpResponseMessage GetSingle(int id)
        {
            var item = Repository.GetByKey(id);

            if (item == null)
                return NotFound();

            return Ok(Map(item));
        }

        protected virtual TModel PostQuery(TModel item)
        {
            Repository.Insert(item);
            Repository.Save();

            return item;
        }

        // POST api/T
        public virtual HttpResponseMessage Post(TInputDTO dto)
        {
            try
            {
                var item = Map<TInputDTO, TModel>(dto);

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
        public virtual HttpResponseMessage Put(int id, TInputDTO dto)
        {
            try
            {
                var oldItem = Repository.GetByKey(id);
                if (!HasWriteAccess(oldItem, KitosUser)) return Unauthorized();

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
                if (!HasWriteAccess(item, KitosUser)) return Unauthorized();

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
                if (!HasWriteAccess(item, KitosUser)) return Unauthorized();

                var itemType = item.GetType();

                foreach (var valuePair in obj)
                {
                    // get name of mapped property
                    var map =
                        AutoMapper.Mapper.FindTypeMapFor<TInputDTO, TModel>()
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

        protected virtual bool HasWriteAccess(TModel obj, User user)
        {
            if (obj.ObjectOwnerId.HasValue && obj.ObjectOwnerId == user.Id) return true;

            return false;
        }
    }
}
