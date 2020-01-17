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
            $scope.KLEUpdateAvailableLabel = "Undersøger om der er en ny version af KLE...";
            toggleKleButtonsClickAbility(false, false);
            KLEservice.getStatus().success(function (dto, status) {
                    if (status !== 200) {
                        notify.addErrorMessage("Der skete en fejl ifm. tjek af ny KLE version");
                        return;
                    }
                    console.log(dto.response);
                    console.log(dto.response.upToDate);
                    if (!dto.response.upToDate) {
                        $scope.KLEUpdateAvailableLabel = "Der er en ny version af KLE, udgivet " + dto.response.version;
                        toggleKleButtonsClickAbility(true, false);
                    }
                    else {
                        $scope.KLEUpdateAvailableLabel = "KITOS baserer sig på den seneste KLE version, udgivet  " + dto.response.version;
                        toggleKleButtonsClickAbility(false, false);
                    }
                }).
                error(() => {
                    toggleKleButtonsClickAbility(false, false);
                    notify.addErrorMessage("Der skete en fejl ifm. tjek af ny KLE version");
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
            toggleKleButtonsClickAbility(false, false);
            KLEservice.getChanges().success((data, status) => {
                if (status !== 200)
                {
                    toggleKleButtonsClickAbility(true, false);
                    notify.addErrorMessage("Der skete en fejl under hentning af ændringer");
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
                    toggleKleButtonsClickAbility(true, true);
                }).
                error(() => {
                    toggleKleButtonsClickAbility(true, false);
                    notify.addErrorMessage("Der skete en fejl under henting af ændringer");
                });
        }

        $scope.UpdateKLE = function () {
            toggleKleButtonsClickAbility(false, false);
            if (confirm("Sikker på at du vil opdatere KLE til nyeste version?")) {
                angular.element("body").append('<div id="overlay" style="background-color:grey;position:absolute;top:0;left:0;height:100%;width:100%;z-index:99999;opacity:0.9;text-align:center;justify-content: center;flex-direction: column;display: flex;"><h1 id=loadingDots>Opdaterer KLE vent venligst</h1></div>');
                var animation = setInterval(updateAnimationStart, 1000);
                KLEservice.applyUpdateKLE().
                    success((data, status) => {
                        if (status !== 200) {
                            toggleKleButtonsClickAbility(true, false);
                            clearInterval(animation);
                            angular.element(document.getElementById("overlay")).remove();
                            notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                            return;
                        }
                        notify.addSuccessMessage("KLE er opdateret");
                        clearInterval(animation);
                        angular.element(document.getElementById("overlay")).remove();
                        getKleStatus();
                    }).
                    error(() => {
                        toggleKleButtonsClickAbility(true, false);
                        notify.addErrorMessage("Der skete en fejl under opdatering af KLE");
                        clearInterval(animation);
                        angular.element(document.getElementById("overlay")).remove();
                    });
            } else {
                notify.addInfoMessage("KLE opdatering stoppet!");
            }
            
        }

        function toggleKleButtonsClickAbility(updateAvailButton: boolean, updateButton: boolean) {
            $scope.KleUpdateAvailableButtonInteraction = updateAvailButton;
            $scope.KleApplyUpdateButtonInteraction = updateButton;
        }

        function updateAnimationStart() {

            var loading = document.getElementById("loadingDots");

            if (loading.innerHTML.length > 29) {
                loading.innerHTML = "Opdaterer KLE vent venligst";
            }
            else {
                loading.innerText += ".";
            }


        }


    }]);
})(angular, app);