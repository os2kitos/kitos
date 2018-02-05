((ng, app) => {
    app.config(["$stateProvider", $stateProvider => {
        $stateProvider.state("it-system.usage.GDPR", {
            url: "/GDPR",
            templateUrl: "app/components/it-system/usage/tabs/it-system-usage-tab-GDPR.view.html",
            controller: "system.GDPR",
            resolve: {
                systemCategories: [
                    "$http", $http => $http.get("odata/LocalItSystemCategories?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                    .then(result => result.data.value)
                ],
                systemUsage: [
                    "$http", "$stateParams", ($http, $stateParams) =>
                    $http.get(`odata/itSystemUsages(${$stateParams.id})`)
                    .then(result => result.data)
                ],
                regularSensitiveData: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                    $http.get(`odata/GetRegularPersonalDataByObjectID(id=${$stateParams.id})`)
                    .then(result => result.data.value)
                ],
                sensitivePersonalData: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                    $http.get(`odata/GetSensitivePersonalDataByObjectID(id=${$stateParams.id})`)
                    .then(result => result.data.value)
                ],
                registerTypes: [
                    '$http', '$stateParams', ($http, $stateParams) =>
                    $http.get(`odata/GetRegisterTypesByObjectID(id=${$stateParams.id})`)
                    .then(result => result.data.value)
                ]
            }
        });
    }]);

    app.controller("system.GDPR",
        [
            "$scope", "$http", "$state", "$stateParams", "$timeout", "itSystemUsage", "itSystemUsageService", "systemUsage", "systemCategories", "moment", "notify", "registerTypes", "regularSensitiveData", "sensitivePersonalData",
            ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, itSystemUsageService, systemUsage, systemCategories, moment, notify, registerTypes, regularSensitiveData, sensitivePersonalData) => {

            $scope.usage = itSystemUsage;
            $scope.registerTypes = registerTypes;
  
            //inherit from parent if general purpose is empty
            $scope.generalPurpose = $scope.usage.generalPurpose;

            if (!$scope.generalPurpose) {
                $scope.generalPurpose = $scope.usage.itSystem.generalPurpose;
            }

            $scope.updateUrl = '/api/itsystemusage/' + $scope.usage.id;



            $scope.updateDataLevel = function (OptionId, Checked, optionType) {

                var msg = notify.addInfoMessage("Arbejder ...", false);

                if (Checked == true) {

                    var data = {
                        ObjectId: itSystemUsage.id,
                        OptionId: OptionId,
                        OptionType: optionType
                    };

                    $http.post("Odata/AttachedOptions/", data, { handleBusy: true }).success(result => {
                        msg.toSuccessMessage("Feltet er Opdateret.");
                    }).error(() => {
                        msg.toErrorMessage("Fejl!");
                    });

                } else {
                    $http.delete("Odata/RemoveOption(id=" + OptionId + ", objectId=" + itSystemUsage.id + ",type='" + optionType + "')").success(() => {
                        msg.toSuccessMessage("Feltet er Opdateret.");
                    }).error(() => {
                        msg.toErrorMessage("Fejl!");
                    });
                }
            }
console.log($scope.usage);
            console.log(systemUsage);

            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.systemCategories = systemCategories;
            $scope.regularSensitiveData = regularSensitiveData;
            $scope.sensitivePersonalData = sensitivePersonalData;
            //$scope.dataProcessor = systemUsage.dataProcessor;
            //$scope.dataProcessingAgreement = systemUsage.dataProcessingAgreement;
            //$scope.dataProcessorControl = systemUsage.dataProcessorControl;
            //$scope.lastControl = systemUsage.lastControl;
            $scope.selection = [];
            $scope.persOptions = [
                'Kryptering',
                'Pseudonymisering',
                'Dataminimering',
                'Logning',
                'Rettigheds- og adgangsstyring'
            ];

            $scope.patch = (field, value) => {
                var payload = {};
                payload[field] = value;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }

            $scope.patchDate = (field, value) => {
                var date = moment(value);

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    $scope.ArchivedDate = systemUsage.ArchivedDate;
                } else {
                    date = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = date;
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                    $scope.ArchivedDate = date;
                }
            }

            $scope.toggleSelection = data => {
                var idx = $scope.selection.indexOf(data);
                // Is currently selected
                if (idx > -1) {
                    $scope.selection.splice(idx, 1);
                }
                
                // Is newly selected
                else {
                    $scope.selection.push(data);
                }
            };
            //$scope.patch = (field, value) => {
            //    var payload = {};
            //    payload[field] = value;
            //    itSystemUsageService.patchSystem($scope.usageId, payload);
            //}

            //$scope.patchDate = (field, value) => {
            //    var date = moment(value);

            //    if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
            //        notify.addErrorMessage("Den indtastede dato er ugyldig.");
            //        $scope.lastControl = $scope.lastControl;
            //    } else {
            //        date = date.format("YYYY-MM-DD");
            //        var payload = {};
            //        payload[field] = date;
            //        itSystemUsageService.patchSystem($scope.usageId, payload);
            //        $scope.lastControl = date;
            //    }
            //}

            $scope.datepickerOptions = {
                format: "yyyy-MM-dd"
            };
        }]);

})(angular, app);
