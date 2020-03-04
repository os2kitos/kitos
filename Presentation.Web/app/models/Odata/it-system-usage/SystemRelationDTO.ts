module Kitos.Models.Odata.ItSystemUsage {

    export interface ISystemRelationDTO {
        Id: number,
        FromSystemUsageId: number,
        ToSystemUsageId: number,
        AssociatedContractId?: number,
        Description?: string,
        RelationInterfaceId?: number,
        Reference?: string,
        UsageFrequenceId?: number,
        UUID: string,
    }
}