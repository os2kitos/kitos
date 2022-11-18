using System;
using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class StsOrganizationChangeLogResponseDTO
    {
        public StsOrganizationChangeLogResponseDTO(StsOrganizationChangeLogOriginOption origin, string? name, DateTime logTime, IEnumerable<StsOrganizationConsequenceLog> consequences)
        {
            Origin = origin;
            Name = name;
            LogTime = logTime;
            Consequences = consequences;
        }

        public StsOrganizationChangeLogOriginOption Origin { get;  }
        public string? Name { get; set; }
        public DateTime LogTime { get; set; }
        public IEnumerable<ConnectionUpdateOrganizationUnitConsequenceResponseDTO> Consequences { get; set; }

        private IEnumerable<ConnectionUpdateOrganizationUnitConsequenceResponseDTO> MapLogsToDto(
            IEnumerable<StsOrganizationConsequenceLog> logs)
        {
            return logs.GetEnumerator()
        }

        private ConnectionUpdateOrganizationUnitConsequenceResponseDTO MapLogToDto(StsOrganizationConsequenceLog log)
        {
            return new ConnectionUpdateOrganizationUnitConsequenceResponseDTO
            {
                Uuid = log.Uuid,
                Name = log.Name,
                //TODO: Add mapping
                Category = log.Type
            }
        }
    }
}