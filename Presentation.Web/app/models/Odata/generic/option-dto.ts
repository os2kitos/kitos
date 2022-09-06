module Kitos.Models.OData.Generic {

    export interface IOptionDTO<TReference extends IEntity> extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<TReference>;
    }
}