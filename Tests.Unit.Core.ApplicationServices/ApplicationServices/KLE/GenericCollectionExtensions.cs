using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Unit.Core.ApplicationServices.KLE
{
    public static class GenericCollectionExtensions
    {
        public static ICollection<T> Clone<T>(this ICollection<T> listToClone) where T: ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}