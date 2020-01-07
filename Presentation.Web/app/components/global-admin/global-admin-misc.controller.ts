(function (ng, app) {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("global-admin.misc", {
            url: "/misc",
            templateUrl: "app/components/global-admin/global-admin-misc.view.html",
            controller: "globalAdminMisc",
            authRoles: ["GlobalAdmin"],
            resolve: {
                globalConfigs: [
                    "$http", $http => $http.get("/odata/GlobalConfigs").then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("globalAdminMisc", ["$rootScope", "$scope", "$http", "uploadFile", "globalConfigs", "_", "notify", ($rootScope, $scope, $http, uploadFile, globalConfigs, _, notify) => {
        $rootScope.page.title = "Andet";
        $scope.KLEupdateReady = false;

        if ($scope.KLEupdateReady) {
            $scope.KLEUpdateAvailableLabel = "KLE Opdatering er klar!";
        } 
        else 
        {
            $scope.KLEUpdateAvailableLabel = "KLE kører med nyeste version! (1.2.3)";
        }

        $scope.canGlobalAdminOnlyEditReports = _.find(globalConfigs, function (g) {
            return g.key === "CanGlobalAdminOnlyEditReports";
        });

        $scope.uploadFile = function () {
            var fileToBeUploaded = $scope.myFile;
            uploadFile.uploadFile(fileToBeUploaded);
        };

        $scope.patchConfig = function (config) {
            var payload = {};
            payload["value"] = config.value;

            $http.patch("/odata/GlobalConfigs(" + config.Id + ")", payload).then(function (newUser) {
                notify.addSuccessMessage("Feltet er opdateret!");
            }, function () {
                notify.addErrorMessage("Fejl! Kunne ikke opdatere feltet!");
            });
        };

        $scope.GetKLEChanges = function() {
            notify.addSuccessMessage("Henter ændringer");
        }

        $scope.UpdateKLE = function () {
            if (confirm("Sikker på at du vil opdatere KLE til nyeste version?")) {

                notify.addSuccessMessage("KLE er nu opdateret!");
                
            } else {
                notify.addInfoMessage("KLE opdatering stoppet!");
            }
        }
    }]);
})(angular, app);