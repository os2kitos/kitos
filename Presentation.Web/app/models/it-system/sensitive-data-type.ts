module Kitos.Models.ItSystem {
    export interface ISensitiveDataType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<ItSystemUsage.IItSystemUsage>;
    }
}
