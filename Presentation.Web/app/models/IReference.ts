module Kitos.Models {
    export interface IReference extends IEntity {
        Itcontract_Id: number;
        ItSystemUsage_Id: number;
        Title: string;
        ExternalReferenceId: string;
        URL: string;
        Display: string;
        Created: Date;
    }

    export interface BaseReference {
        Title: string;
        ExternalReferenceId: string;
        URL: string;
        Created: Date;
    }
}