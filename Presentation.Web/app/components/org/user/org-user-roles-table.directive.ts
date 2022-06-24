module Kitos.Organization.Users {
    export interface IOrgUserRolesTableCallbacks<T> {
        selectionChanged(): void;
        delete(right : T);
    }

    ((ng, app) => {
        'use strict';

        app.directive("orgUserRolesTable",
            [
                () => ({
                    templateUrl: "app/components/org/user/org-user-roles-table.view.html",
                    scope: {
                        id: "@",
                        model: "=ngModel",
                        roleTypeName: "@",
                        disabled: "=ngDisabled",
                        callbacks: "=callbacks",
                        selectedUser: "=",
                    }
                })
            ]);
    })(angular, app);
}