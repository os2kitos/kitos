module Kitos.ExcelExport.Modal.Create {
    "use strict";

    export function createModalInstance(_, $modal) {
        return $modal.open({
            windowClass: "modal fade in",
            templateUrl: "app/components/excel-export/excel-export-modal-view.html",
            backdrop: "static",
            controller: [
                () => {
                   /* $scope.hasWriteAccess = hasWriteAccess;
                    $scope.selectedReceivers = [];
                    $scope.selectedCCs = [];
                    $scope.adviceTypeData = null;
                    $scope.adviceRepetitionData = null;
                    $scope.adviceTypeOptions = Models.ViewModel.Advice.AdviceTypeOptions.options;
                    $scope.adviceRepetitionOptions = Models.ViewModel.Advice.AdviceRepetitionOptions.options;
                    $scope.startDateInfoMessage = null;*/
                   console.log("TEST");
                }
            ],
            resolve: {
                type: [() => $scope.type],
                action: [() => $scope.action],
                object: [() => $scope.object],
                currentUser: [
                    "userService",
                    (userService: Kitos.Services.IUserService) => userService.getUser()
                ]
            }
        });
    }
}