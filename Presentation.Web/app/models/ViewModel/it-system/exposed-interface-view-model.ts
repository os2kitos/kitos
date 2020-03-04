module Kitos.Models.ViewModel.ItSystem {

    export interface IGenericDescriptionViewModel {
        description: string;
        longText: boolean;
        shortTextLineCount: number;
    }

    export interface IGenericUrlViewModel {
        value: string;
        isValid: boolean;
    }

    export interface IExposedInterfaceViewModel {
        name: string,
        interfaceName: string,
        url: IGenericUrlViewModel,
        description: IGenericDescriptionViewModel,
    }

    export class ExposedInterfaceViewModel implements IExposedInterfaceViewModel {
        name: string;
        interfaceName: string;
        url: IGenericUrlViewModel;
        description: IGenericDescriptionViewModel;

        constructor(
            maxTextFieldCharCount: number,
            shortTextLineCount: number,
            currentExposedInterface: any) {

            this.name = currentExposedInterface.name;
            this.interfaceName = currentExposedInterface.interfaceName;

            const des = currentExposedInterface.description;
            if (des != null) {
                this.description = <IGenericDescriptionViewModel>{};
                this.description.description = des;
                this.description.longText = des.length > maxTextFieldCharCount;
                this.description.shortTextLineCount = shortTextLineCount;
            }

            const url = currentExposedInterface.url;
            if (url != null) {
                this.url = <IGenericUrlViewModel>{};
                this.url.value = url;
                this.url.isValid = Utility.Validation.validateUrl(url);
            }
        }
    }


}