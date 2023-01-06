module Kitos.Organization.Users {
    export interface IOrgUserRolesTableCallbacks<T> {
        selectionChanged(): void;
        delete(right: T);
        selectOrDeselectGroup(rights: Models.ViewModel.Organization.IHasSelection[], areAllSelected: boolean): void;
    }

    function setupComponent(): ng.IComponentOptions {
        return {
            bindings: {
                id: "@",
                model: "=ngModel",
                roleTypeName: "@",
                disabled: "=ngDisabled",
                callbacks: "=callbacks",
                selectedUser: "="
            },
            controller: OrgUserRolesTableController,
            controllerAs: "ctrl",
            templateUrl: `app/shared/organization-tree/organization-tree.view.html`
        };
    }

    export interface IAssignedRightViewModel extends Models.ViewModel.Organization.IHasSelection {
        right: Models.Users.IAssignedRightDTO;
    }

    export interface IRootAssignedRightsWithGroupSelectionViewModel extends Models.ViewModel.Organization.IHasSelection {
        rights: IAssignedRightViewModel[];
    }

    interface IOrgUserRolesTableController extends ng.IComponentController {
        model: IRootAssignedRightsWithGroupSelectionViewModel | null;
    }

    class OrgUserRolesTableController implements IOrgUserRolesTableController {
        model: IRootAssignedRightsWithGroupSelectionViewModel | null = null;

        constructor() {

        }

        $onInit?(): void {
            if (this.model == null) {
                console.error("No model provided in OrgUserRolesTableController");
            }
            this.model.rights = this.model.rights.sort((a, b) => a.right.businessObjectName.localeCompare(b.right.businessObjectName, Kitos.Shared.Localization.danishLocale));
        }
    }

    angular.module("app")
        .component("orgUserRolesTable", setupComponent());
}