module Kitos.Models.ViewModel.GDPR {
    export interface IOversightDateViewModel {
        id: number;
        oversightDate: string;
        oversightRemark: string;
    }

    export class OversightDateViewModel implements IOversightDateViewModel {
        id: number;
        oversightDate: string;
        oversightRemark: string;

        constructor(id: number, oversightDate: string, oversightRemark: string) {
            if (id === null || oversightDate === null || oversightRemark === null) {
                throw new Error("inputs cannot be null");
            }
            this.id = id;
            this.oversightDate = oversightDate;
            this.oversightRemark = oversightRemark;
        }
    }
}