using System.Collections.Generic;
using Core.DomainModel.ItSystem.DataTypes;

namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public class GDPRExportReport
    {
        public string SystemName { get; set; }
        public bool NoData { get; set; }
        public bool PersonalData { get; set; }
        public bool SensitiveData { get; set; }
        public bool LegalData { get; set; }
        public DataOptions? BusinessCritical { get; set; }
        public bool DataProcessingAgreementConcluded { get; set; }
        public bool LinkToDirectory { get; set; }
        public IEnumerable<string> SensitiveDataTypes { get; set; }
        public DataOptions? RiskAssessment { get; set; }
        public RiskLevel? PreRiskAssessment { get; set; }
        public DataOptions? DPIA { get; set; }
        public HostedAt? HostedAt { get; set; }
    }
}
