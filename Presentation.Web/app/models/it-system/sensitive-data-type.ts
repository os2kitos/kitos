module Kitos.Models.ItSystem {
    export interface ISensitiveDataType extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<ItSystemUsage.IItSystemUsage>;
    }
}
