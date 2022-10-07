((ng, app) => {
    'use strict';

    app.directive("singleSelectSelect2", [
        () => ({
            templateUrl: "app/shared/singleSelect/singleSelectSelect2.view.html",
            scope: {
                id: "@",
                placeholder: "@",
                model: "=ngModel",
                appendurl: "@",
                disabled: "=ngDisabled",
                required: "@"
            },
            link(scope) {

                scope.optionDescription = null;

                scope.$watch("model.selectedElement",
                    value => scope.optionDescription = value?.optionalObjectContext?.description);
            }
        })
    ]);
})(angular, app);
