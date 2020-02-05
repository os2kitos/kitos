module Kitos.Models.ItSystemUsage.Relation {
    export interface IItSystemUsageRelationDTO {

        FromUsageId: number;
        ToUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;
    }
}