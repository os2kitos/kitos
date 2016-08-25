module Kitos.Models {
    export interface IReport extends IEntity {
        Name: string;
        Description: string;
        CategoryTypeId: number;
        CategoryType: IOptionEntity;
        OrganizationId: number;
        /** The organization which the unit belongs to. */
        Organization: any;
        Definition: string;
    }

     export interface IReportCategory extends IEntity {
         
     }
}