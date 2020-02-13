module Kitos.Models.ItSystemUsage.Relation {
    export interface IItSystemUsageCreateRelationDTO {
        FromUsageId: number;
        ToUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;
    }

    import NamedEntityDTO = Models.Generic.NamedEntity.NamedEntityDTO;

    export interface IItSystemUsageRelationDTO {
        id: number;
        fromUsage: NamedEntityDTO;
        toUsage: NamedEntityDTO;
        interface: NamedEntityDTO;
        contract: NamedEntityDTO;
        frequencyType: NamedEntityDTO;
        description: string;
        reference: string;
    }

    export interface IItSystemUsageRelationOptionsDTO {
        availableInterfaces: [NamedEntityDTO];
        availableContracts: [NamedEntityDTO];
        availableFrequencyTypes: [NamedEntityDTO];
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

    export interface ISystemRelationSelectionModel {
        value?: NamedEntityDTO;
        options: NamedEntityDTO[];
    }

    export interface ISystemRelationModalIdText {
        id: number;
        text: string;
    }

    export interface ISystemRelationModalViewModel {
        fromSystem: NamedEntityDTO;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;
    }

    export interface ISystemRelationPatchDTO {
        Id: number;
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
        fromSystem: NamedEntityDTO;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationSelectionModel;
        contract: ISystemRelationSelectionModel;
        frequency: ISystemRelationSelectionModel;
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;

        constructor(fromSystemId: number, fromSystemName: string) {
            this.fromSystem = <NamedEntityDTO>{ id: fromSystemId, name: fromSystemName };
            this.toSystem = null;
            this.interface = <ISystemRelationSelectionModel>{};
            this.contract = <ISystemRelationSelectionModel>{};
            this.frequency = <ISystemRelationSelectionModel>{};
            this.reference = <ISystemRelationModalIdText>{};
            this.description = <ISystemRelationModalIdText>{};
        }

        setValuesFrom(relationData: IItSystemUsageRelationDTO) {
            this.bindValue(this.frequency, relationData.frequencyType);
            this.bindValue(this.contract, relationData.contract);
            this.bindValue(this.interface, relationData.interface);
            this.id = relationData.id;
            this.description.text = relationData.description;
            this.reference.text = relationData.reference;

        }

        updateAvailableOptions(optionsResult: IItSystemUsageRelationOptionsDTO) {
            // Build modal with data
            this.bindOptions(this.frequency, optionsResult.availableFrequencyTypes);
            this.bindOptions(this.interface, optionsResult.availableInterfaces);
            this.bindOptions(this.contract, optionsResult.availableContracts);
        }

        setTargetSystem(id: number, name: string) {
            this.toSystem = <ISystemRelationModalIdText>{ id: id, text: name };
        }

        private bindValue(targetData: ISystemRelationSelectionModel, sourceData: NamedEntityDTO) {
            if (sourceData) {
                targetData.value = <NamedEntityDTO>{ id: sourceData.id, name: sourceData.name };
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

    export interface EditISystemRelationSelectionModel {
        value?: NamedEntityDTO;
        options: NamedEntityDTO[];
    }

    export interface EditISystemRelationModalViewModel {
        fromSystem: NamedEntityDTO;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationModalIdText;
        interfaceOptions: ISystemRelationSelectionModel,
        contract: ISystemRelationModalIdText;
        contractOptions: ISystemRelationSelectionModel,
        frequency: ISystemRelationModalIdText;
        frequencyOptions: ISystemRelationSelectionModel,
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;
    }

    export class EditSystemRelationModalViewModel implements EditISystemRelationModalViewModel {
        id: number;
        fromSystem: NamedEntityDTO;
        toSystem: ISystemRelationModalIdText;
        interface: ISystemRelationModalIdText;
        interfaceOptions: ISystemRelationSelectionModel;
        contract: ISystemRelationModalIdText;
        contractOptions: ISystemRelationSelectionModel;
        frequency: ISystemRelationModalIdText;
        frequencyOptions: ISystemRelationSelectionModel;
        reference: ISystemRelationModalIdText;
        description: ISystemRelationModalIdText;

        constructor(fromSystemId: number, fromSystemName: string) {
            this.fromSystem = <NamedEntityDTO>{ id: fromSystemId, name: fromSystemName };
            this.toSystem = null;
            this.interface = null;
            this.contract = null;
            this.frequency = null;
            this.interfaceOptions = <ISystemRelationSelectionModel>{};
            this.contractOptions = <ISystemRelationSelectionModel>{};
            this.frequencyOptions = <ISystemRelationSelectionModel>{};
            this.reference = <ISystemRelationModalIdText>{};
            this.description = <ISystemRelationModalIdText>{};
        }

        setValuesFrom(relationData: IItSystemUsageRelationDTO) {
            this.frequency = this.bindValue(relationData.frequencyType);
            this.contract = this.bindValue(relationData.contract);
            this.interface = this.bindValue(relationData.interface);
            this.id = relationData.id;
            this.description.text = relationData.description;
            this.reference.text = relationData.reference;

        }

        updateAvailableOptions(optionsResult: IItSystemUsageRelationOptionsDTO) {
            // Build modal with data
            this.bindOptions(this.frequencyOptions, optionsResult.availableFrequencyTypes);
            this.bindOptions(this.interfaceOptions, optionsResult.availableInterfaces);
            this.bindOptions(this.contractOptions, optionsResult.availableContracts);
        }

        setTargetSystem(id: number, name: string) {
            this.toSystem = <ISystemRelationModalIdText>{ id: id, text: name };
        }

        private bindValue(sourceData: NamedEntityDTO) {
            if (sourceData) {
                return <ISystemRelationModalIdText>{ id: sourceData.id, text: sourceData.name };
            } else {
                return null;
            }
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

    export class SystemRelationModelPostDataObject implements IItSystemUsageCreateRelationDTO {
        ToUsageId: number;
        FromUsageId: number;
        Description: string;
        InterfaceId?: number;
        FrequencyTypeId?: number;
        ContractId?: number;
        Reference: string;

        constructor(relationModelToCreate: EditISystemRelationModalViewModel) {

            this.ToUsageId = relationModelToCreate.toSystem.id;
            this.FromUsageId = relationModelToCreate.fromSystem.id;
            this.Description = relationModelToCreate.description.text;
            this.Reference = relationModelToCreate.reference.text;

            this.InterfaceId = relationModelToCreate.interface.id;
            this.ContractId = relationModelToCreate.contract.id;
            this.FrequencyTypeId = relationModelToCreate.frequency.id;
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
        Description: string;
        Reference: string;
        FromUsage: IItSystemUsageRelationIdName;
        ToUsage: IItSystemUsageRelationIdName;
        Interface?: IItSystemUsageRelationIdName;
        Contract?: IItSystemUsageRelationIdName;
        FrequencyType?: IItSystemUsageRelationIdName;

        constructor(data: EditSystemRelationModalViewModel) {

            this.Id = data.id;
            this.Description = data.description.text;
            this.Reference = data.reference.text;
            this.FromUsage = { Id: data.fromSystem.id, Name: data.fromSystem.name };
            this.ToUsage = { Id: data.toSystem.id, Name: data.toSystem.text };
            this.FrequencyType = { Id: data.frequency.id, Name: data.frequency.text };
            this.Interface = { Id: data.interface.id, Name: data.interface.text };
            this.Contract = { Id: data.contract.id, Name: data.contract.text };

        }

        private setValuesOrNull(value: NamedEntityDTO) {
            if (value !== null) {
                return <IItSystemUsageRelationIdName>{ Id: value.id, Name: value.name }
            } else {
                return null;
            }
        }


    }
}