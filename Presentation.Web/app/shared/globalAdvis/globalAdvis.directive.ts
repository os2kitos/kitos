(function (ng, app) {
    app.directive("globalAdvis", [
        function () {
            return {
                templateUrl: "app/shared/globalAdvis/it-advice-global.view.html",
                scope: {
                    stateName: "@",
                    user : "="
                },
                controller: [
                    '$scope',
                    '$http',
                    '$uibModal',
                    '$state',
                    '$window',
                    (
                        $scope,
                        $http,
                        $uibModal,
                        $state,
                        $window) => {
                        $scope.$watch("stateName", function (newValue, oldValue) {
                            if ($scope.stateName === "it-project.overview" || $scope.stateName === "it-system.overview" || $scope.stateName === "it-contract.overview" || $scope.stateName === "it-contract.plan" || $scope.stateName === "data-processing.overview" )
                                $scope.disableAdvisLink = false;
                            else
                                $scope.disableAdvisLink = true;
                        });
                        var parent = $scope;
                        
                        $scope.showAdviceModal = () => {
                            var modalInstance = $uibModal.open({
                                windowClass: "modal fade in",
                                templateUrl: "app/shared/globalAdvis/it-advice-global.modal.view.html",
                                size: 'lg',
                                controller: ["$scope", "$uibModalInstance", "notify", function ($scope, $modalInstance, nofity) {
                                    var today = moment().format('YYYY-MM-DD');
                                    var stateUrl = "";
                                    var moduleTypeFilter = "";
                                    if (parent.stateName === "it-project.overview") {
                                        $scope.title = "IT advis - IT Projekter";
                                        moduleTypeFilter = "Type eq 'itProject'";
                                        stateUrl = $window.location.href.replace("overview", "edit");
                                    }
                                    if (parent.stateName === "it-contract.overview" || parent.stateName === "it-contract.plan") {
                                        $scope.title = "IT advis - IT Kontrakter";
                                        moduleTypeFilter = "Type eq 'itContract'";
                                        stateUrl = $window.location.href.replace("overview", "edit");
                                    }
                                    if (parent.stateName === "it-system.overview") {
                                        $scope.title = "IT advis - IT Systemer";
                                        moduleTypeFilter = "Type eq 'itSystemUsage'";
                                        stateUrl = $window.location.href.replace("overview", "usage");
                                    }
                                    if (parent.stateName === "data-processing.overview") {
                                        $scope.title = "IT advis - Databehandleraftaler";
                                        moduleTypeFilter = "Type eq 'dataProcessingAgreement'";
                                        stateUrl = $window.location.href.replace("overview", "edit");
                                    }
                                    $scope.mainGridOptions = {
                                        dataSource: {                                            
                                            type: "odata-v4",
                                            transport: {
                                                read: {
                                                    url: `/Odata/GetAdvicesByOrganizationId(organizationId=${$scope.user.currentOrganizationId})?$filter=${moduleTypeFilter} AND StopDate gt ${today}&$expand=Reciepients, Advicesent`,
                                                    dataType: "json"
                                                },
                                            },
                                            pageSize: 10,
                                            serverPaging: true,
                                            serverFiltering: true,
                                        },
                                        selectable: true,
                                        columns: [{
                                            field: "Name",
                                            title: "Navn",
                                            template: data => {
                                                return `<a ng-click="$dismiss()" href="${stateUrl}/${data.RelationId}/advice">${data.Name}</a>`;
                                            },
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "SentDate",
                                            title: "Sidst sendt",
                                            template: x => {
                                                if (x.SentDate != null) {
                                                    return kendo.toString(new Date(x.SentDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "Id",
                                            hidden: true
                                        },

                                        {
                                            field: "AlarmDate",
                                            title: "Start dato",
                                            template: x => {
                                                if (x.AlarmDate != null) {
                                                    return kendo.toString(new Date(x.AlarmDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "StopDate",
                                            title: "Slut dato",
                                            template: x => {
                                                if (x.StopDate != null) {
                                                    return kendo.toString(new Date(x.StopDate), "d");
                                                }
                                                return "";
                                            },
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "Reciepients.Name", title: "Modtager",
                                            template: () =>
                                                `<span data-ng-model="dataItem.Reciepients" value="cc.Name" ng-repeat="cc in dataItem.Reciepients | filter: { RecieverType: 'RECIEVER'}"> {{cc.Name}}{{$last ? '' : ', '}}</span>`,
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "Reciepients.Name",
                                            title: "CC",
                                            template: () =>
                                                `<span data-ng-model="dataItem.Reciepients" value="cc.Name" ng-repeat="cc in dataItem.Reciepients | filter: { RecieverType: 'CC'}"> {{cc.Name}}{{$last ? '' : ', '}}</span>`,
                                            attributes: { "class": "might-overflow" }
                                        },
                                        {
                                            field: "Subject",
                                            title: "Emne"
                                        },                                        
                                        ],                                        
                                        pageable: {
                                            refresh: true,
                                            pageSizes: [10, 25, 50, 100, 200],
                                            buttonCount: 5
                                        },
                                    }                                                                      
                                }]
                            });
                        }
                    }]
            };
        }
    ]);
})(angular, app);
