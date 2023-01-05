module Kitos.Helpers {
    export interface ISelectDataSource {
        id: number;
        text: string;
        emptyOption?: boolean;
    }
    
    export class KendoOverviewHelper {
        static readonly emptyOptionId = Number.NaN;
        static mapDataForKendoDropdown(dataSource: ISelectDataSource[], insertEmptyOption: boolean): Utility.KendoGrid.IKendoParameter[]{

            if (insertEmptyOption && dataSource.filter(x => x.emptyOption).length === 0) {
                dataSource.unshift({
                    id: this.emptyOptionId,
                    text: "",
                    emptyOption: true
                });
            }

            return dataSource.map(value => {
                    return {
                        textValue: value.text,
                        remoteValue: value.id,
                        optionalContext: value
                    };
            });
        }

        static createActiveRange(isSystemStatus: boolean): Utility.KendoGrid.IKendoParameter[] {
            const texts = Helpers.RenderFieldsHelper.getTexts();
            const statusTexts = isSystemStatus ? texts.status.systems : texts.status.other;
            return [
                {
                    textValue: statusTexts.active,
                    remoteValue: true
                } as Utility.KendoGrid.IKendoParameter,
                {
                    textValue: statusTexts.notActive,
                    remoteValue: false
                } as Utility.KendoGrid.IKendoParameter
            ];
        }
    }
}