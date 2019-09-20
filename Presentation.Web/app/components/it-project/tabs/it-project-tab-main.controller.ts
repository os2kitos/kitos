(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-project.edit.main", {
            url: "/main",
            templateUrl: "app/components/it-project/tabs/it-project-tab-main.view.html",
            controller: "project.EditMainCtrl"
        });
    }]);

    app.controller("project.EditMainCtrl",
        ["$scope", "$http", "notify", "_", "project", "projectTypes", "user", "hasWriteAccess", "moment", "autofocus",
            function ($scope, $http, notify, _, project, projectTypes, user, hasWriteAccess, moment, autofocus) {

                $scope.projectTypes = projectTypes;
                $scope.autosaveUrl = `api/itproject/${project.id}`;
                $scope.moment = moment;
                $scope.hasWriteAccess = hasWriteAccess;
                $scope.project = project;
                $scope.global = user.isGlobalAdmin;
                if ($scope.project.parentId) {
                    $scope.project.parent = {
                        id: $scope.project.parentId,
                        text: $scope.project.parentName
                    };
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
