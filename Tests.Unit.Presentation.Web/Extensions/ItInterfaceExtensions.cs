using Core.DomainModel.ItSystem;

namespace Tests.Unit.Presentation.Web.Extensions
{
    public static class ItInterfaceExtensions
    {
        public static ItInterface ExhibitedBy(this ItInterface src, ItSystem exhibitedBy)
        {
            src.ExhibitedBy = new ItInterfaceExhibit { ItSystem = exhibitedBy, ItSystemId = exhibitedBy.Id };
            exhibitedBy.ItInterfaceExhibits.Add(new ItInterfaceExhibit { ItSystem = exhibitedBy, ItSystemId = exhibitedBy.Id, ItInterface = src });
            return src;
        }
    }
}
