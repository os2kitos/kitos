module Kitos.Models.ItProject {
    export interface IItProjectType extends IEntity {
        Name: string;
        IsActive: boolean;
        Note: string;
        References: Array<IItProject>;
    }
}
