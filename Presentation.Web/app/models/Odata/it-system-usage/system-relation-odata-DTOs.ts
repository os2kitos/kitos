module Kitos.Models.Odata.ItSystemUsage.Relation {

    export interface IItSystemRelationOdataDTO {
        FromSystemUsageId: number;
        Uuid: string;
        ToSystemUsageId: number;
        RelationInterfaceId: number;
        Description: string;
        Reference: string;
        UsageFrequencyId: number;
        AssociatedContractId: number;
    }

}