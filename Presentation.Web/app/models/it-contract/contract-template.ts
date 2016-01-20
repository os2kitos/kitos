module Kitos.Models.ItContract {
   	/** It contract contract template options. */
    export interface IContractTemplate extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        Note: string;
        References: Array<IItContract>;
    }
}
