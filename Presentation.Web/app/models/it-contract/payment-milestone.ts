module Kitos.Models.ItContract {
	/** It contract payment milestone */
    export interface IPaymentMilestone extends IEntity {
        /** Gets or sets the title. */
        Title: string;
        /** Gets or sets the expected date. */
        Expected: Date;
        /** Gets or sets the approved date. */
        Approved: Date;
        /** Gets or sets the associated it contract identifier. */
        ItContractId: number;
        /** Gets or sets the associated it contract. */
        ItContract: IItContract;
    }
}
