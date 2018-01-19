((ng, app) => {
    app.config(["$stateProvider", function ($stateProvider) {
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
                ]
            }
        });
    }]);

    app.controller("system.GDPR",
    [
        "$scope", "$http", "$state", "$stateParams", "$timeout", "itSystemUsage", "itSystemUsageService", "systemUsage", "systemCategories","moment", "notify",
        ($scope, $http, $state, $stateParams, $timeout, itSystemUsage, itSystemUsageService, systemUsage, systemCategories, moment, notify) => {
            $scope.usage = itSystemUsage;
            $scope.usageId = $stateParams.id;
            $scope.systemUsage = systemUsage;
            $scope.systemCategories = systemCategories;
            $scope.dataProcessor = 'Test af data behandler';
            $scope.dataProcessingAgreement = 'Test af databehandler aftale';
            $scope.dataProcessorControl = 2;
            $scope.lastControl = '2017-01-01';
            $scope.catagory = {
                name: 'test navn',
                address: 'test adresse',
                tlf: '123456',
                mail: 'something@kitos.dk',
                CPR: '123456-7890',
                economics: 'the good, the bad and the ugly',
                ethnicity: 'human etnisk',
                politicalAffiliation: 'holdninger',
                religious: 'tro, håb og ....',
                unionized: 'fagforening',
                geneticDate: 'genetisk data',
                biometricDate: 'Bimetrisk data',
                health: 'helbredet',
                SexualOrientation: 'sex it up'
            };
            $scope.selection = [];
            $scope.persOptions = [
                'Kryptering',
                'Pseudonymisering',
                'Dataminimering',
                'Logning',
                'Rettigheds- og adgangsstyring'
            ];

            $scope.toggleSelection = data => {
                console.log(data);
                var idx = $scope.selection.indexOf(data);
                console.log(idx);
                // Is currently selected
                if (idx > -1) {
                    $scope.selection.splice(idx, 1);
                }
                
                // Is newly selected
                else {
                    $scope.selection.push(data);
                }
            };
            $scope.patch = (field, value) => {
                var payload = {};
                payload[field] = value;
                itSystemUsageService.patchSystem($scope.usageId, payload);
            }

            $scope.patchDate = (field, value) => {
                var date = moment(value);

                if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                    notify.addErrorMessage("Den indtastede dato er ugyldig.");
                    $scope.lastControl = $scope.lastControl;
                } else {
                    date = date.format("YYYY-MM-DD");
                    var payload = {};
                    payload[field] = date;
                    itSystemUsageService.patchSystem($scope.usageId, payload);
                    $scope.lastControl = date;
                }
            }

            $scope.datepickerOptions = {
                format: "yyyy-MM-dd"
            };
        }]);

})(angular, app);
