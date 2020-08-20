module Kitos.Models.ViewModel.ItSystem {
    import ArchiveDutyRecommendationFactory = Models.ItSystem.ArchiveDutyRecommendationFactory;

    export interface IArchiveDuty {
        value: string;
        optionalComment: string;
        readMoreLink: string;
    }

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
        readonly archiveDuty: IArchiveDuty,
        readonly uuid: string,
    }

    export class SystemViewModel implements ISystemViewModel {
        readonly name: string;
        readonly disabled : boolean;
        readonly parentName: string;
        readonly previousName: string;
        readonly belongsToName: string;
        readonly accessModifier: string;
        readonly description: string;
        readonly externalReferences: any;
        readonly taskRefs: any;
        readonly businessTypeName: string;
        readonly archiveDuty: IArchiveDuty;
        readonly uuid: string;

        constructor(itSystem: any) {
            this.name = itSystem.name + (itSystem.disabled ? " (Ikke aktiv)" : "");
            if (itSystem.parentName) {
                this.parentName = itSystem.parentName + (itSystem.parentDisabled ? " (Ikke aktiv)" : "");
            }
            this.previousName = itSystem.previousName;
            this.belongsToName = itSystem.belongsToName;
            this.description = itSystem.description;
            this.externalReferences = itSystem.externalReferences;
            this.taskRefs = itSystem.taskRefs;
            this.businessTypeName = itSystem.businessTypeName;
            this.uuid = itSystem.uuid;
            this.disabled = itSystem.disabled;
            this.archiveDuty = this.mapArchiveDuty(itSystem);
            this.accessModifier = Mappers.AccessModifierMapper.mapAccessModifier(itSystem.accessModifier);
        }

        private mapArchiveDuty(system): IArchiveDuty {
            const archiveDuty = ArchiveDutyRecommendationFactory.mapFromNumeric(system.archiveDuty);
            return <IArchiveDuty> {
                value: !archiveDuty || archiveDuty.value < 1 ? null : archiveDuty.name,
                optionalComment: system.archiveDutyComment,
                readMoreLink: Constants.Archiving.ReadMoreUri,
            }
        }
    }
}