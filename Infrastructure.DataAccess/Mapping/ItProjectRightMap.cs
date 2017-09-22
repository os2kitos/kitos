using Core.DomainModel.ItProject;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItProjectRightMap : RightMap<ItProject, ItProjectRight, ItProjectRole>
    {
        public ItProjectRightMap()
        {
            this.HasRequired(x => x.User)
                .WithMany(x => x.ItProjectRights)
                .HasForeignKey(x => x.UserId);
        }
    }
}
