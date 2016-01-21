module Kitos.Models.ItContract {
	/** It contract role option. */
    export interface IItContractRole extends IEntity {
        Name: string;
        IsActive: boolean;
        IsSuggestion: boolean;
        HasReadAccess: boolean;
        HasWriteAccess: boolean;
        Note: string;
        References: Array<IItContractRight>;
        /** Gets or sets the receivers for an advice. */
        ReceiverFor: Array<IAdvice>;
        /** Gets or sets the carbon copy receivers for an advice. */
        CarbonCopyReceiverFor: Array<IAdvice>;
    }
}
