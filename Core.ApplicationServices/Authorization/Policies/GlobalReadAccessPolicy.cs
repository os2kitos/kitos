using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.Authorization.Policies
{
    public class GlobalReadAccessPolicy : IAuthorizationPolicy<Type>
    {
        //NOTE: For types which cannot be bound to a scoped context (lack of knowledge) and has shared read access
        private static readonly IReadOnlyDictionary<Type, bool> TypesWithGlobalReadAccess;

        static GlobalReadAccessPolicy()
        {
            var typesWithGlobalRead =
                new Dictionary<Type, bool>
                {
                    {typeof(Advice),true},
                    {typeof(AdviceUserRelation),true},
                    {typeof(Text),true},
                    {typeof(HelpText),true},
                    {typeof(AdviceSent),true},
                    {typeof(GlobalConfig),true },
                    {typeof(ExternalReference),true },
                    {typeof(TaskRef),true }
                };

            //All base options are globally readable
            typeof(Entity)
                .Assembly
                .GetTypes()
                .Where(t => t.IsImplementationOfGenericType(typeof(OptionEntity<>)))
                .Where(t => t.IsAbstract == false)
                .Where(t => t.IsInterface == false)
                .ToList()
                .ForEach(t => typesWithGlobalRead.Add(t, true));

            TypesWithGlobalReadAccess = new ReadOnlyDictionary<Type, bool>(typesWithGlobalRead);
        }
        public bool Allow(Type target)
        {
            return target
                .FromNullable()
                .Select(TypesWithGlobalReadAccess.ContainsKey)
                .GetValueOrFallback(false);
        }
    }
}
