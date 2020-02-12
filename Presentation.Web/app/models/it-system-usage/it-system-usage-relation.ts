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

    export interface ISystemRelationModalIdName {
        id: number;
        name: string;
    }

    export interface ISystemRelationSelectionModel {
        value?: ISystemRelationModalIdName;
        options: ISystemRelationModalIdName[];
    }

    export interface ISystemRelationModalIdText {
        id: number;
        text: string;
    }

    export interface ISystemRelationModalViewModel {
        fromSystem: ISystemRelationModalIdName;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;
    }

    export interface ISystemRelationPatchDTO {
        Id: number;
        Uuid: string;
        Description: string;
        Reference: string;
        FromUsage: IItSystemUsageRelationIdName;
        ToUsage: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Contract?: IItSystemUsageRelationIdName;
        FrequencyType?: IItSystemUsageRelationIdName;

    }

    export interface ISystemRelationViewModel {
        RelationId: number;
        FromSystem: IItSystemUsageRelationIdName;
        ToSystem: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Description: IItSystemUsageRelationDescriptionDto;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;
    }

    export interface ISystemGetRelationDTO {
        id: number;
        uuid: string;
        fromUsage: ISystemRelationModalIdName;
        toUsage: ISystemRelationModalIdName;
        interface?: ISystemRelationModalIdName;
        contract?: ISystemRelationModalIdName;
        frequencyType?: ISystemRelationModalIdName;
        description: string;
        reference: string;
    }

    export class SystemRelationViewModel implements ISystemRelationViewModel {
        RelationId: number;
        FromSystem: IItSystemUsageRelationIdName;
        ToSystem: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Description: IItSystemUsageRelationDescriptionDto;
        Reference: IItSystemUsageRelationReferenceDto;
        Contract?: IItSystemUsageRelationIdName;
        Frequency?: IItSystemUsageRelationIdName;

        private mapNameAndId(viewModel: IItSystemUsageRelationIdName, sourceModel: Models.Generic.NamedEntity.NamedEntityDTO) {
            if (sourceModel != null) {
                viewModel.Name = sourceModel.name;
                viewModel.Id = sourceModel.id;
            }
        }

        private mapSystemUsage(viewModel: IItSystemUsageRelationIdName, sourceModel: Models.Generic.NamedEntity.NamedEntityDTO) {
            if (sourceModel != null) {
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

            this.RelationId = currentRelation.id;

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

    export class SystemRelationModalViewModel implements ISystemRelationModalViewModel {
        id: number;
        uuid: string;
        fromSystem: ISystemRelationModalIdName;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;

        constructor(fromSystemId: number, fromSystemName: string) {
            this.fromSystem = <ISystemRelationModalIdName>{ id: fromSystemId, name: fromSystemName };
            this.toSystem = <ISystemRelationModalIdText>{};
            this.interface = <ISystemRelationSelectionModel>{};
            this.contract = <ISystemRelationSelectionModel>{};
            this.frequency = <ISystemRelationSelectionModel>{};
            this.reference = <ISystemRelationModalIdText>{};
            this.description = <ISystemRelationModalIdText>{};
        }

        setValuesFrom(relationData: ISystemGetRelationDTO) {
            this.bindValue(this.frequency, relationData.frequencyType);
            this.bindValue(this.contract, relationData.contract);
            this.bindValue(this.interface, relationData.interface);
            this.id = relationData.id;
            this.uuid = relationData.uuid;
            this.description.text = relationData.description;
            this.reference.text = relationData.reference;

        }

        updateAvailableOptions(optionsResult: any /*TODO:not the any - use strong types*/) {
            // Build modal with data
            this.bindOptions(this.frequency, optionsResult.response.availableFrequencyTypes);
            this.bindOptions(this.interface, optionsResult.response.availableInterfaces);
            this.bindOptions(this.contract, optionsResult.response.availableContracts);
        }

        setTargetSystem(id: number, name: string) {
            this.toSystem.id = id;
            this.toSystem.text = name;
        }

        private bindValue(targetData: ISystemRelationSelectionModel, sourceData: ISystemRelationModalIdName) {
            if (sourceData) {
                targetData.value = <ISystemRelationModalIdName>{ id: sourceData.id, name: sourceData.name };
            } else {
                targetData.value = null;
            }
            this.bindOptions(targetData, targetData.options);
        }

        private bindOptions(targetData: ISystemRelationSelectionModel, sourceData: any) {
            let selectedValue = targetData.value;
            targetData.options = sourceData;
            targetData.value = null;

            //Set selected value to previously selected value if it was selected before
            if (selectedValue) {
                targetData.options = sourceData;
                for (let i = 0; i < sourceData.length; i++) {

                    let optionExists = selectedValue.id === sourceData[i].id;
                    if (optionExists) {
                        targetData.value = sourceData[i];
                        break;
                    }
                }
            }
        }
    }

    export class SystemRelationModelPostDataObject implements IItSystemUsageRelationDTO {
        ToUsageId: number;
        FromUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;

        constructor(relationModelToCreate: ISystemRelationModalViewModel) {

            this.ToUsageId = relationModelToCreate.toSystem.id;
            this.FromUsageId = relationModelToCreate.fromSystem.id;
            this.Description = relationModelToCreate.description.text;
            this.Reference = relationModelToCreate.reference.text;

            this.InterfaceId = this.getIdFromValues(relationModelToCreate.interface);
            this.ContractId = this.getIdFromValues(relationModelToCreate.contract);
            this.FrequencyTypeId = this.getIdFromValues(relationModelToCreate.frequency);
        }

        private getIdFromValues(valuesToInsert: ISystemRelationSelectionModel) {
            if (valuesToInsert.value !== null) {
                return valuesToInsert.value.id;
            } else {
                return null;
            }
        }
    }

    export class SystemRelationModelPatchDataObject implements ISystemRelationPatchDTO {
        Id: number;
        Uuid: string;
        Description: string;
        Reference: string;
        FromUsage: IItSystemUsageRelationIdName;
        ToUsage: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Contract?: IItSystemUsageRelationIdName;
        FrequencyType?: IItSystemUsageRelationIdName;

        constructor(data: SystemRelationModalViewModel) {

            this.Id = data.id;
            this.Uuid = data.uuid;
            this.Description = data.description.text;
            this.Reference = data.reference.text;
            this.FromUsage = { Id: data.fromSystem.id, Name: data.fromSystem.name };
            this.ToUsage = { Id: data.toSystem.id, Name: data.toSystem.text };
            this.FrequencyType = this.setValuesOrNull(data.frequency.value);
            this.Interface = this.setValuesOrNull(data.interface.value);
            this.Contract = this.setValuesOrNull(data.contract.value);

        }

        private setValuesOrNull(value: ISystemRelationModalIdName) {
            if (value !== null) {
                return <IItSystemUsageRelationIdName>{ Id: value.id, Name: value.name }
            } else {
                return null;
            }
        }


    }
}