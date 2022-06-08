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

        public static removeItemFromToolbarByName(name: string, items: IKendoGridToolbarItem[]) {

            const itemsWithMatchingName = items.filter(x => x.name === name);
            if (itemsWithMatchingName.length < 1)
                return;

            const item = itemsWithMatchingName[0];
            const index = items.indexOf(item);
            items.splice(index);
        }
    }
}


