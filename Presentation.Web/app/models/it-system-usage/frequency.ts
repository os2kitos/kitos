module Kitos.Models.ItSystemUsage {
    export interface IFrequency extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
    }
}
