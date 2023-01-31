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
                callbacks: "=callbacks",
                selectedUser: "="
            },
            controller: OrgUserRolesTableController,
            controllerAs: "ctrl",
            templateUrl: "app/components/org/user/user-delete-components/org-user-roles-table.view.html"
        };
    }

    export interface IAssignedRightViewModel extends Models.ViewModel.Organization.IHasSelection {
        right: Models.Users.IAssignedRightDTO;
    }

    export interface IRootAssignedRightsWithGroupSelectionViewModel extends Models.ViewModel.Organization.IHasSelection {
        rights: IAssignedRightViewModel[];
    }

    interface IOrgUserRolesTableController extends ng.IComponentController {
        model: IRootAssignedRightsWithGroupSelectionViewModel | null
        callbacks: IOrgUserRolesTableCallbacks<unknown> | null
        roleTypeName: string | null
        selectedUser: Models.IUser | null
    }

    class OrgUserRolesTableController implements IOrgUserRolesTableController {
        model: IRootAssignedRightsWithGroupSelectionViewModel | null = null;
        callbacks: IOrgUserRolesTableCallbacks<unknown> | null = null;
        roleTypeName: string | null = null;
        selectedUser: Models.IUser | null = null;

        $onInit?(): void {
            if (this.model == null) {
                console.error("No model provided in OrgUserRolesTableController");
            }
            if (this.callbacks == null) {
                console.error("No callbacks provided in OrgUserRolesTableController");
            }
            if (this.roleTypeName == null) {
                console.error("No roleTypeName provided in OrgUserRolesTableController");
            }
            if (this.selectedUser == null) {
                console.error("No selectedUser provided in OrgUserRolesTableController");
            }
            this.model.rights = this.model.rights.sort((a, b) => {
                    const businessObjectComp = a.right.businessObjectName.localeCompare(b.right.businessObjectName, Shared.Localization.danishLocale);
                    if (businessObjectComp !== 0) return businessObjectComp;

                    return a.right.roleName.localeCompare(b.right.roleName, Shared.Localization.danishLocale);
            });
        }
    }

    angular.module("app")
        .component("orgUserRolesTable", setupComponent());
}