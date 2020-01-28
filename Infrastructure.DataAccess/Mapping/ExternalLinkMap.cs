using Core.DomainModel;

namespace Infrastructure.DataAccess.Mapping
{
    public class ExternalLinkMap  :EntityMap<ExternalLink>
    {
        public ExternalLinkMap()
        {
            this.Property(x => x.Name).IsOptional();
            this.Property(x => x.Url).IsOptional();
        }
    }
}
