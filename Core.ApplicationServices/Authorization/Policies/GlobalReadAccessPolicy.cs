﻿using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Infrastructure.Services.Types;


namespace Core.ApplicationServices.Authorization.Policies
{
    public class GlobalReadAccessPolicy : IGlobalReadAccessPolicy
    {
        //NOTE: For types which cannot be bound to a scoped context (lack of knowledge) and has shared read access
        private static readonly ISet<Type> TypesWithGlobalReadAccess;

        static GlobalReadAccessPolicy()
        {
            var typesWithGlobalRead =
                new HashSet<Type>
                {
                    typeof(Text),
                    typeof(HelpText),
                    typeof(ExternalReference),
                    typeof(TaskRef)
                };

            //All base options are globally readable
            typeof(Entity)
                .Assembly
                .GetTypes()
                .Where(t => t.IsImplementationOfGenericType(typeof(OptionEntity<>)))
                .Where(t => t.IsAbstract == false)
                .Where(t => t.IsInterface == false)
                .ToList()
                .ForEach(t => typesWithGlobalRead.Add(t));

            TypesWithGlobalReadAccess = new HashSet<Type>(typesWithGlobalRead);
        }
        public bool Allow(Type target)
        {
            return target
                .FromNullable()
                .Select(TypesWithGlobalReadAccess.Contains)
                .GetValueOrFallback(false);
        }
    }
}
