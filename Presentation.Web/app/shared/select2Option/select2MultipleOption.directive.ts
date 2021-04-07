(function (ng, app) {
    'use strict';

    app.controller("select2MultipleOptionController", ["$scope", "select2LoadingService", function ($scope, select2LoadingService) {
        var availableOptions: Kitos.Models.ViewModel.Generic.Select2OptionViewModel<null>[] = $scope.options;
        var selectedOptions: Kitos.Models.ViewModel.Generic.Select2OptionViewModel<null>[] = $scope.selected;

        var selectedNoLongerAvailable = _.differenceWith(selectedOptions, availableOptions, (selected, available) => {
            return selected.id === available.id;
        });

        _.forEach(selectedNoLongerAvailable, (selected) => {
            availableOptions.splice(0, 0, { id: selected.id, text: selected.text + " (Slettes)", optionalObjectContext: null });
            addToBeDeleted(selectedOptions, selected.id);
        });


        $scope.select2Config = select2LoadingService.select2MultipleLocalDataNoSearch(() => availableOptions, true);

        $scope.selectedIds = selectedOptions;

        function addToBeDeleted(input: any, id: number) {
            var match = _.find(input, function (item: any) { return item.id === id });
            if (match) {
                _.merge(match, { id: match.id, text: match.text + " (slettes)" });
            }
        }

    }]);

    app.directive("select2MultipleOption", [
        function () {
            return {
                templateUrl: "app/shared/select2Option/select2MultipleOption.view.html",
                scope: {
                    id: "@",
                    label: "@",
                    placeholder: "@",
                    options: "=",
                    selectedIds: "=ngModel",
                    selected: "=",
                    disabled: "=ngDisabled",
                    required: "@"
                },
                controller: "select2MultipleOptionController",
                link: function (scope, element, attr, ctrl) {

                    scope.$watch(attr.disabled, function (newVal) {
                        element.prop('disabled', newVal);
                    });
                }
            };
        }
    ]);
})(angular, app);