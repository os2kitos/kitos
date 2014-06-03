using System.Collections.Generic;

namespace Core.DomainModel
{
    public class ExtReferenceType : Entity, IOptionEntity<ExtReference>, IHasLocales<ExtRefTypeLocale>
    {
        public ExtReferenceType()
        {
            this.References = new List<ExtReference>();
            this.Locales = new List<ExtRefTypeLocale>();
            this.IsActive = true;
            this.IsSuggestion = false;
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ExtReference> References { get; set; }
        public virtual ICollection<ExtRefTypeLocale> Locales { get; set; }
    }
}
