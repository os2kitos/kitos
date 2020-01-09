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

    app.controller("globalAdminMisc", ["$rootScope", "$scope", "$http", "uploadFile", "globalConfigs", "_", "notify", "KLEservice", ($rootScope, $scope, $http, uploadFile, globalConfigs, _, notify, KLEservice) => {
        $rootScope.page.title = "Andet";
        $scope.KLEupdateReadyStep1 = false;
        $scope.KLEupdateReadyStep2 = false;

        KLEservice.getStatus().success(dto => {

            if (!dto.response.uptodate) {
                $scope.KLEUpdateAvailableLabel = "KLE Opdatering er klar! " + dto.response.version;
                $scope.KLEupdateReadyStep1 = true;
                $scope.KLEupdateReadyStep2 = false;
            }
            else {
                $scope.KLEUpdateAvailableLabel = "KLE kører med nyeste version! " + dto.response.version;
            }
        });

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

        $scope.GetKLEChanges = function () {
            //TODO Query for database changes in xml format.

         //   KLEservice.getChanges().success(csv => {
           //     $scope.KLEupdateReadyStep2 = true;
             //   notify.addSuccessMessage("Henter ændringer");
            //});

            KLEservice.getChanges().success((data) => {
                    var anchor = angular.element('<a/>');
                    anchor.attr({
                        href: 'data:attachment/csv;charset=utf-8,' + encodeURI(data),
                        target: '_blank',
                        download: 'filename.csv'
                    })[0].click();

                }).
                error(function (data, status, headers, config) {
                    // handle error
                });


        }

        $scope.UpdateKLE = function () {
            if (confirm("Sikker på at du vil opdatere KLE til nyeste version?")) {
                //TODO Query database to update
                notify.addSuccessMessage("KLE er nu opdateret!");
                
            } else {
                notify.addInfoMessage("KLE opdatering stoppet!");
            }
        }
    }]);
})(angular, app);