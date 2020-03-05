module Kitos.Models.ViewModel.ItSystem {

    import IGenericUrlViewModel = ViewModel.Generic.IGenericUrlViewModel;
    import IGenericDescriptionViewModel = ViewModel.Generic.IGenericDescriptionViewModel;

    export interface IExposedInterfaceViewModel {
        id: number,
        name: string,
        interfaceName: string,
        url: IGenericUrlViewModel,
        description: IGenericDescriptionViewModel,
        disabled: boolean,
    }

    export class ExposedInterfaceViewModel implements IExposedInterfaceViewModel {
        id: number;
        name: string;
        interfaceName: string;
        url: IGenericUrlViewModel;
        description: IGenericDescriptionViewModel;
        disabled: boolean;

        constructor(
            maxTextFieldCharCount: number,
            shortTextLineCount: number,
            currentExposedInterface: any) {

            this.id = currentExposedInterface.id;
            this.name = currentExposedInterface.name;
            this.interfaceName = currentExposedInterface.interfaceName;
            this.disabled = currentExposedInterface.disabled;

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