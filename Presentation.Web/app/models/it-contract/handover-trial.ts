module Kitos.Models.ItContract {
    export interface IHandoverTrial extends IEntity {
        Expected: Date;
        Approved: Date;
        ItContractId: number;
        ItContract: IItContract;
        HandoverTrialTypeId: number;
        HandoverTrialType: IHandoverTrialType;
    }
}
