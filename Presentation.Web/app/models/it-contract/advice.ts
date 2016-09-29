module Kitos.Models.ItContract {
    /** Contains info about Advices on a contract. */
    export interface IAdvice extends IEntity {
        /** Gets or sets a value indicating whether this instance is active. */
        IsActive: boolean;
        /** Gets or sets the contract name. */
        Name: string;
        /** Gets or sets the advice alarm date. */
        AlarmDate: Date;
        /** Gets or sets the sent date. */
        SentDate: Date;
        /** Gets or sets the receiver contract role identifier. */
        ReceiverId: number;
        /** Gets or sets the receiver contract role. */
        Receiver: IRoleEntity;
        /** Gets or sets the carbon copy receiver contract role identifier. */
        CarbonCopyReceiverId: number;
        /** Gets or sets the carbon copy contract role receiver. */
        CarbonCopyReceiver: IRoleEntity;
        /** Gets or sets the subject of the email. */
        Subject: string;
        /** Gets or sets it contract identifier. */
        ItContractId: number;
        /** Gets or sets it contract. */
        ItContract: IItContract;
    }
}
