((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.main", {
            url: "/main",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-main.view.html",
            controller: "system.EditMain",
            resolve: {
                systemCategories: [
                    "$http", $http => $http.get("odata/LocalItSystemCategories?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("system.EditMain", ["$rootScope", "$scope", "$http", "notify", "user", "systemCategories", "autofocus",
        ($rootScope, $scope, $http, notify, user, systemCategories, autofocus) => {
            var itSystemUsage = new Kitos.Models.ViewModel.ItSystemUsage.SystemUsageViewModel($scope.usage);
            $rootScope.page.title = "IT System - Anvendelse";
            $scope.autoSaveUrl = `api/itSystemUsage/${itSystemUsage.id}`;
            $scope.hasViewAccess = user.currentOrganizationId === itSystemUsage.organizationId;
            $scope.systemCategories = systemCategories;
            $scope.shouldShowCategories = systemCategories.length > 0;
            $scope.system = new Kitos.Models.ViewModel.ItSystem.SystemViewModel(itSystemUsage.itSystem);
            autofocus();
            $scope.isValidUrl = (url: string) => Kitos.Utility.Validation.validateUrl(url);

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            $scope.patchDate = (field, value) => {
                var expirationDate = itSystemUsage.expirationDate;
                var concluded = itSystemUsage.concluded;
                var formatString = "DD-MM-YYYY";
                var formatDateString = "YYYY-MM-DD";
                var fromDate = moment(concluded, [formatString, formatDateString]).startOf("day");
                var endDate = moment(expirationDate, [formatString, formatDateString]).endOf("day");
                var date = moment(value, "DD-MM-YYYY");
                if (value === "" || value == undefined) {
                    var payload = {};
                    payload[field] = null;
                    patch(payload, $scope.autoSaveUrl + "?organizationId=" + user.currentOrganizationId);
                } else if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                }
                else if (fromDate != null && endDate != null && fromDate >= endDate) {
                    notify.addErrorMessage("Den indtastede slutdato er før startdatoen.");
                }
                else {
                    checkIfActive();

                    var dateString = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = dateString;
                    patch(payload, $scope.autoSaveUrl + "?organizationId=" + user.currentOrganizationId);
                }
            }

            function patch(payload: any, url: any);
            function patch(payload, url) {
                var msg = notify.addInfoMessage("Gemmer...", false);
                $http({ method: "PATCH", url: url, data: payload })
                    .success(() => {
                        msg.toSuccessMessage("Feltet er opdateret.");
                    })
                    .error(() => {
                        msg.toErrorMessage("Fejl! Feltet kunne ikke ændres!");
                    });
            }

            $scope.checkSystemValidity = () => {
                checkIfActive();
            }

            function checkIfActive() {
                const today = moment();

                if (today.isBetween(moment(itSystemUsage.concluded, "DD-MM-YYYY").startOf("day"), moment(itSystemUsage.expirationDate, "DD-MM-YYYY").endOf("day"), null, "[]") ||
                    (today > moment(itSystemUsage.concluded, "DD-MM-YYYY").startOf("day") && itSystemUsage.expirationDate == null) ||
                    (today < moment(itSystemUsage.expirationDate, "DD-MM-YYYY").endOf("day") && itSystemUsage.concluded == null) ||
                    (itSystemUsage.expirationDate == null && itSystemUsage.concluded == null)) {
                    itSystemUsage.isActive = true;
                }
                else {
                    if (itSystemUsage.active) {
                        itSystemUsage.isActive = true;
                    }
                    else {
                        itSystemUsage.isActive = false;
                    }
                }
            }
        }
    ]);
})(angular, app);
