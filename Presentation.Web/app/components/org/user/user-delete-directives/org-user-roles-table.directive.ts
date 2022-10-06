﻿module Kitos.Organization.Users {
    export interface IOrgUserRolesTableCallbacks<T> {
        selectionChanged(): void;
        delete(right: T);
        selectOrDeselectGroup(rights: Models.ViewModel.Organization.IHasSelection[], areAllSelected: boolean): void;
    }

    ((ng, app) => {
        'use strict';

        app.directive("orgUserRolesTable",
            [
                () => ({
                    templateUrl: "app/components/org/user/user-delete-directives/org-user-roles-table.view.html",
                    scope: {
                        id: "@",
                        model: "=ngModel",
                        roleTypeName: "@",
                        disabled: "=ngDisabled",
                        callbacks: "=callbacks",
                        selectedUser: "="
                    }
                })
            ]);
    })(angular, app);
}