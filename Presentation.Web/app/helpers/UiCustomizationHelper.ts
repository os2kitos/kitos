module Kitos.Helpers {

    export class UiCustomizationHelper {

        public static removeUnavailableColumns<TDataSource>(columns: IKendoGridColumn<TDataSource>[]) {
            columns.forEach(column => {
                if (column.isAvailable === undefined || column.isAvailable)
                    return;

                const index = columns.indexOf(column);
                columns.splice(index);
            });
        }
    }
}


