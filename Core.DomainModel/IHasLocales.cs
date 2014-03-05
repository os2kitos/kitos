using System.Collections.Generic;

namespace Core.DomainModel
{
    public interface IHasLocales<TLocale>
    {
        ICollection<TLocale> Locales { get; set; }
    }
}