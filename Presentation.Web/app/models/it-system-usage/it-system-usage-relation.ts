module Kitos.Models.ItSystemUsage.Relation {
    export interface IItSystemUsageRelationDTO {

        FromUsageId: number;
        ToUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;
    }

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface IItSystemUsageRelationReferenceDto {
        Reference: string;
        ValidUrl: boolean;
        LongText: boolean;
        ShortTextLineCount: number;
    }

    export interface IItSystemUsageRelationDescriptionDto {
        Description: string;
        LongText: boolean;
        ShortTextLineCount: number;
    }

    export interface IItSystemUsageRelationIdName {
        Id: number;
        Name: string;
    }

    export interface ISystemRelationViewModel {
        FromSystem: IItSystemUsageRelationIdName;
        ToSystem: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Description: IItSystemUsageRelationDescriptionDto;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;
    }


    export class SystemRelationViewModel implements ISystemRelationViewModel {
        FromSystem: IItSystemUsageRelationIdName;
        ToSystem: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Description: IItSystemUsageRelationDescriptionDto;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;

        private  mapNameAndId(viewModel: IItSystemUsageRelationIdName, sourceModel: Models.Generic.NamedEntity.NamedEntityDTO) {
            if (sourceModel != null) {
                viewModel.Name = sourceModel.name;
                viewModel.Id = sourceModel.id;
            }
        }

        private mapSystemUsage(viewModel: IItSystemUsageRelationIdName, sourceModel: Models.Generic.NamedEntity.NamedEntityDTO) {
            if (sourceModel != null) {
                //viewModel.Url = "#/system/usage/" + sourceModel.id + "/main";
                this.mapNameAndId(viewModel, sourceModel);
            }
        }

        private isValidHyperLink(ref: string) {
            if (ref !== null) {
                return Utility.Validation.validateUrl(ref);
            }
            return false;
        }

        constructor(
            maxTextFieldCharCount: number,
            shortTextLineCount: number,
            currentRelation: any) {

            // From System
            this.FromSystem = <IItSystemUsageRelationIdName>{};
            this.mapSystemUsage(this.FromSystem, currentRelation.fromUsage);

            // To System
            this.ToSystem = <IItSystemUsageRelationIdName>{};
            this.mapSystemUsage(this.ToSystem, currentRelation.toUsage);

            // interface
            if (currentRelation.interface != null) {
                this.Interface = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Interface, currentRelation.interface);
            }
            // Description

            var des = currentRelation.description;
            if (des != null) {
                this.Description = <IItSystemUsageRelationDescriptionDto>{};
                this.Description.Description = des;
                this.Description.LongText = des.length > maxTextFieldCharCount;
                this.Description.ShortTextLineCount = shortTextLineCount;
            }

            //Reference
            const reference = currentRelation.reference;
            if (reference != null) {
                this.Reference = <IItSystemUsageRelationReferenceDto>{};
                if (this.isValidHyperLink(reference)) {
                    this.Reference.ValidUrl = true;
                    this.Reference.Reference = reference;
                } else {
                    this.Reference.Reference = reference;
                    this.Reference.LongText = reference.length > maxTextFieldCharCount;
                    this.Reference.ShortTextLineCount = shortTextLineCount;
                }
            }
            // Contract
            if (currentRelation.contract != null) {
                this.Contract = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Contract, currentRelation.contract);
            }
            // Frequency
            if (currentRelation.frequencyType !== null) {
                this.Frequency = <IItSystemUsageRelationIdName>{};
                this.mapNameAndId(this.Frequency, currentRelation.frequencyType);    
            }
        }
    }
}