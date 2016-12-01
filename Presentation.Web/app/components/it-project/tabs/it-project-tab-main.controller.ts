(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.main", {
            url: "/main",
            templateUrl: "app/components/it-project/tabs/it-project-tab-main.view.html",
            controller: "project.EditMainCtrl",
            resolve: {
                statusUpdates: [
                    "$http", "$stateParams",
                    ($http, $stateParams) => $http.get(`odata/ItProjects(${$stateParams.id})?$expand=ItProjectStatusUpdates($orderby=Created desc;$expand=ObjectOwner($select=Name,LastName))`)
                        .then(result => {
                            return result.data.ItProjectStatusUpdates;
                        })
                ]
            }
        });
    }]);

    app.controller("project.EditMainCtrl",
        ["$scope", "$http", "_", "project", "projectTypes", "user", "hasWriteAccess", "moment", "autofocus", "statusUpdates",
            function ($scope, $http, _, project, projectTypes, user, hasWriteAccess, moment, autofocus, statusUpdates) {
                $scope.autosaveUrl = `api/itproject/${project.id}`;
                $scope.moment = moment;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.project = project;

                if ($scope.project.parentId) {
                    $scope.project.parent = {
                        id: $scope.project.parentId,
                        text: $scope.project.parentName
                    };
                }

                init();

                function init() {
                    $scope.methodOptions = [{ label: 'Samlet', val: true }, { label: 'Tid, kvalitet og ressourcer', val: false }];

                    $scope.allStatusUpdates = statusUpdates;

                    if ($scope.allStatusUpdates.length > 0) {
                        $scope.currentStatusUpdate = $scope.allStatusUpdates[0];
                        $scope.showCombinedChart = ($scope.currentStatusUpdate.IsCombined) ? $scope.methodOptions[0] : $scope.methodOptions[1];
                    }

                    $scope.combinedStatusUpdates = _.filter($scope.allStatusUpdates, function (s: any) { return s.IsCombined; });
                    $scope.splittedStatusUpdates = _.filter($scope.allStatusUpdates, function (s: any) { return !s.IsCombined; });
                }

                $scope.onSelectStatusMethod = function (showCombined) {
                    if (showCombined) {
                        $scope.currentStatusUpdate = ($scope.combinedStatusUpdates.length > 0) ? $scope.combinedStatusUpdates[0] : null;
                    } else {
                        $scope.currentStatusUpdate = ($scope.splittedStatusUpdates.length > 0) ? $scope.splittedStatusUpdates[0] : null;
                    }
                }

                $scope.selectLazyLoading = function(url, excludeSelf, paramAry) {
                    return {
                        minimumInputLength: 1,
                        allowClear: true,
                        placeholder: " ",
                        initSelection: () => {
                        },
                        ajax: {
                            data: (term) => {
                                return { query: term };
                            },
                            quietMillis: 500,
                            transport: (queryParams) => {
                                var extraParams = paramAry ? "&" + paramAry.join("&") : "";
                                var res = $http.get(url + "?q=" + queryParams.data.query + extraParams).then(queryParams.success);
                                // res.abort = () => null;

                                return res;
                            },

                            results: (data: { data}) => {
                                var results = [];

                                _.each(data.data.response, (obj: { id; name; cvr; }) => {
                                    if (excludeSelf && obj.id == $scope.project.id)
                                        return; // don't add self to result

                                    results.push({
                                        id: obj.id,
                                        text: obj.name ? obj.name : "Unavngiven",
                                        cvr: obj.cvr
                                    });
                                });

                                return { results: results };
                            }
                        }
                    };
                }

                $scope.parentSelectOptions = $scope.selectLazyLoading("api/itproject", true, ["overview=true", `orgId=${user.currentOrganizationId}`]);
            }]);
})(angular, app);
