module Kitos.Models.ViewModel.GlobalAdmin.Organization {

    export interface IOrganizationModalViewModelData {
        name: string;
        accessmodifier: string;
        cvr?: number;
        typeId: number;
        foreignCvr?: number;
    }

    export interface IOrganizationModalViewModel {
        title: string;
        data: IOrganizationModalViewModelData;
    }

    export class OrganizationModalViewModel implements IOrganizationModalViewModel
    {
        title: string;
        data: IOrganizationModalViewModelData;

        constructor() {
            this.title = "";
            this.data = <IOrganizationModalViewModelData>{
                name: "",
                accessmodifier: "1",
                cvr: null,
                typeId: 1,
                foreignCvr: null
            };
        }

        configureAsNewOrganizationDialog() {
            this.title = "Opret Opret organisation";
        }

        configureAsEditOrganizationDialog(name: string, accessmodifier: string, cvr: number, typeId: number, foreignCvr: number) {
            this.title = "Rediger organisation";
            this.data.name = name;
            this.data.accessmodifier = accessmodifier;
            this.data.cvr = cvr;
            this.data.typeId = typeId;
            this.data.foreignCvr = foreignCvr;
        }
    }
}