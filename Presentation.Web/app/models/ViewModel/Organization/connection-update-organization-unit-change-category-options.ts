module Kitos.Models.ViewModel.Organization {
    export class ConnectionUpdateOrganizationUnitChangeCategoryOptions {
        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                    var text = "";

                    switch (parseInt(next) as Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory) {
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Added:
                            text = "Tilføjet";
                            break;
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Converted:
                            text = "Konverteret";
                            break;
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Deleted:
                            text = "Slettet";
                            break;
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Moved:
                            text = "Flyttet";
                            break;
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.Renamed:
                            text = "Omdøbt";
                            break;
                        case Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory.RootChanged:
                            text = "Organisationsrod erstattet";
                            break;
                        default:
                            text = "Ukendt:" + next;
                            break;
                    }
                    //Set by numeric and text value
                    acc[next] = text;
                    acc[Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory[next]] = text;
                    return acc;
                },
                    {}
                );
        }

        //Cache the names for quick lookup
        private static readonly valueToNameMap = ConnectionUpdateOrganizationUnitChangeCategoryOptions.getValueToTextMap();

        static getText(option: Models.Api.Organization.ConnectionUpdateOrganizationUnitChangeCategory) {
            return ConnectionUpdateOrganizationUnitChangeCategoryOptions.valueToNameMap[option];
        }        
    }
}