module Kitos.Models.ItSystem {
    /** Dropdown option for ItSystem, whether it has been archived or not. */
    export interface IArchivePeriode extends IEntity {
        UniqueArchiveId: string;
        StartDate: Date;
        EndDate: Date;
    }
}
