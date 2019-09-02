using Core.DomainModel.Reports;

namespace Infrastructure.DataAccess.Mapping
{
    public class ReportMap : EntityMap<Report>
    {
        public ReportMap()
        {
            TypeMapping.AddIndexOnAccessModifier<ReportMap, Report>(this);
        }
    }
}