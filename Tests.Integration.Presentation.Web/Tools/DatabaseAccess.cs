using System;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Infrastructure.DataAccess;

namespace Tests.Integration.Presentation.Web.Tools
{
    /// <summary>
    /// NOTE: Keep use of this to a minimum. Prefer changing KITOS data through the KITOS API
    /// </summary>
    public static class DatabaseAccess
    {
        /// <summary>
        /// Helper method to perform queries on entities in a repository.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="map"></param>
        /// <returns></returns>
        public static TOutput MapFromEntitySet<TModel, TOutput>(Func<IGenericRepository<TModel>, TOutput> map) where TModel : class
        {
            using var repository = new GenericRepository<TModel>(TestEnvironment.GetDatabase());
            
            return map(repository);
        }

        /// <summary>
        /// Helper method to perform changes to entities in a repository.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="mutate"></param>
        public static void MutateEntitySet<TModel>(Action<IGenericRepository<TModel>> mutate) where TModel : class
        {
            using var kitosContext = TestEnvironment.GetDatabase();
            using var repository = new GenericRepository<TModel>(kitosContext);

            mutate(repository);

            repository.Save();
        }

        /// <summary>
        /// Helper method to perform changes to entities in a repository.
        /// </summary>
        /// <param name="mutate"></param>
        public static void MutateDatabase(Action<KitosContext> mutate)
        {
            using var db = TestEnvironment.GetDatabase();

            mutate(db);
        }

        /// <summary>
        /// Map an entity db id to the corresponding uuid used outside of kitos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbId"></param>
        /// <returns></returns>
        public static Guid GetEntityUuid<T>(int dbId) where T : class, IHasUuid, IHasId
        {
            return DatabaseAccess.MapFromEntitySet<T, Guid>(x => x.AsQueryable().ById(dbId).Uuid);
        }
    }
}
