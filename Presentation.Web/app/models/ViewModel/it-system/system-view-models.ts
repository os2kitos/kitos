module Kitos.Models.ViewModel.ItSystem {

    export interface ISystemViewModel {
        readonly name: string,
        readonly parentName: string,
        readonly previousName: string,
        readonly belongsToName: string,
        readonly accessModifier: string,
        readonly description: string,
        readonly externalReferences: any,
        readonly taskRefs: any,
        readonly businessTypeName: string,
        readonly archiveDuty: string,
        readonly uuid: string,
    }

    export class SystemViewModel implements ISystemViewModel {
        readonly name: string;
        readonly parentName: string;
        readonly previousName: string;
        readonly belongsToName: string;
        readonly accessModifier: string;
        readonly description: string;
        readonly externalReferences: any;
        readonly taskRefs: any;
        readonly businessTypeName: string;
        readonly archiveDuty: string;
        readonly uuid: string;

        constructor(itSystem: any) {
            this.name = itSystem.name;
            this.parentName = itSystem.parentName;
            this.previousName = itSystem.previousName;
            this.belongsToName = itSystem.belongsToName;
            this.description = itSystem.description;
            this.externalReferences = itSystem.externalReferences;
            this.taskRefs = itSystem.taskRefs;
            this.businessTypeName = itSystem.businessTypeName;
            this.uuid = itSystem.uuid;

            this.archiveDuty = this.mapArchiveDuty(itSystem.archiveDuty);
            this.accessModifier = this.mapAccessModifier(itSystem.accessModifier);
        }

        private mapAccessModifier(accessModifier) {
            switch (accessModifier) {
                case 0:
                    return "Lokal";
                case 1:
                    return "Offentlig";
                default:
                    return null;
            }
        }

        private mapArchiveDuty(archiveDuty) {
            switch (archiveDuty) {
                case 1:
                    return "B";
                case 2:
                    return "K";
                case 3:
                    return "Ved ikke";
            default:
            }
        }
    }
}