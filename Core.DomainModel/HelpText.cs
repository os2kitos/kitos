using Core.Abstractions.Types;

namespace Core.DomainModel
{
    public class HelpText : Entity
    {
        public string Title { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }

        public void UpdateTitle(Maybe<string> title)
        {
            Title = title.HasValue ? title.Value : null;
        }
        public void UpdateDescription(Maybe<string> description)
        {
            Description = description.HasValue ? description.Value : null;
        }
    }
}
