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

    app.controller("globalAdminMisc", ["$rootScope", "$scope", "$http", "uploadFile", "globalConfigs", "_", "notify", "KLEservice", "$window", ($rootScope, $scope, $http, uploadFile, globalConfigs, _, notify, KLEservice, $window) => {
        $rootScope.page.title = "Andet";
        getKleStatus();
        function getKleStatus() {
            $scope.KLEUpdateAvailableLabel = "Tjekker efter ny version af KLE";
            $scope.KleUpdateAvailableButtonInteraction = false;
            $scope.KleApplyUpdateButtonInteraction = false;
            KLEservice.getStatus().success((dto, status) => {
                    if (status !== 200) {
                        notify.addErrorMessage("Der skete en fejle under tjek af version!");
                        return;
                    }
                    if (!dto.response.uptodate) {
                        $scope.KLEUpdateAvailableLabel = "Der er en ny version af KLE, udgivet " + dto.response.version;
                        $scope.KleUpdateAvailableButtonInteraction = true;
                        $scope.KleApplyUpdateButtonInteraction = false;
                    }
                    else {
                        $scope.KLEUpdateAvailableLabel = "KITOS baserer sig på den seneste KLE version, udgivet  " + dto.response.version;
                        $scope.KleUpdateAvailableButtonInteraction = false;
                        $scope.KleApplyUpdateButtonInteraction = false;
                    }
                }).
                error(() => {
                    $scope.KleUpdateAvailableButtonInteraction = false;
                    $scope.KleApplyUpdateButtonInteraction = false;
                    notify.addErrorMessage("Der skete en fejl ved tjekke om der er opdatering klar.");
                });
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

        $scope.GetKLEChanges = function () {
            $scope.KleUpdateAvailableButtonInteraction = false;
            $scope.KleApplyUpdateButtonInteraction = false;

            
            KLEservice.getChanges().success((data, status) => {
                if (status !== 200)
                {
                    notify.addErrorMessage("Der skete en fejle under hentning af ændringer fejlkode: " + status);
                    return;
                }
                var universalBOM = "\uFEFF";
                var anchor = angular.element(document.getElementById("KLEDownloadAnchor"));
                anchor.attr("data-element-type", "KLEDownloadAnchor");
                anchor.attr({
                        href: 'data:text/csv; charset=utf-8,' + encodeURI(universalBOM + data),
                        target: "_blank",
                        download: "KLE-Changes.csv"
                    })[0].click();
                    notify.addSuccessMessage("Download af ændringer færdig");
                $scope.KleUpdateAvailableButtonInteraction = true;
                $scope.KleApplyUpdateButtonInteraction = true;
                }).
                error(() => {
                    $scope.KleUpdateAvailableButtonInteraction = true;
                    $scope.KleApplyUpdateButtonInteraction = false;
                    notify.addErrorMessage("Der skete en fejle under henting af ændringer");
                });
        }

        $scope.UpdateKLE = function () {
            if (confirm("Sikker på at du vil opdatere KLE til nyeste version?")) {
                KLEservice.applyUpdateKLE().
                    success((data, status) => {
                        if (status !== 200) {
                            $scope.KleUpdateAvailableButtonInteraction = true;
                            $scope.KleApplyUpdateButtonInteraction = false;
                            notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                            return;
                        }
                        $scope.KleUpdateAvailableButtonInteraction = true;
                        $scope.KleApplyUpdateButtonInteraction = false;
                        notify.addSuccessMessage("KLE er opdateret");
                        getKleStatus();
                    }).
                    error(() => {
                        $scope.KleUpdateAvailableButtonInteraction = true;
                        $scope.KleApplyUpdateButtonInteraction = false;
                        notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                    });
            } else {
                notify.addInfoMessage("KLE opdatering stoppet!");
            }
        }
    }]);
})(angular, app);