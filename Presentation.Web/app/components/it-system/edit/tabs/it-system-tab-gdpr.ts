(function (ng, app) {
    app.config(["$stateProvider", function ($stateProvider) {
        $stateProvider.state("it-system.edit.gdpr", {
            url: "/gdpr",
            templateUrl: "app/components/gdpr-views/gdpr.view.html",
            controller: "system.EditGdpr",
            resolve: {
                theSystem: ['$http', '$stateParams', function ($http, $stateParams) {
                return $http.get("api/itsystem/" + $stateParams.id)
                    .then(function (result) {
                        return result.data.response;
                        });
                }],
                regularSensitiveData: ['$http', '$stateParams','theSystem', function ($http, $stateParams, theSystem) {
                    return $http.get("odata/GetRegularPersonalDataByObjectID(id=" + $stateParams.id + ")")
                        .then(function (result) {
                            return result.data.value;
                        });
                }],
                sensitivePersonalData: ['$http', '$stateParams', function ($http, $stateParams) {
                    return $http.get("odata/GetSensitivePersonalDataByObjectID(id=" + $stateParams.id + ")")
                        .then(function (result) {
                            return result.data.value;
                        });
                }]
            }
        });
    }]);

    app.controller("system.EditGdpr", ["$scope", "$http", "$timeout", "$state", "$stateParams", "$confirm", "notify", "hasWriteAccess", "theSystem", "regularSensitiveData","sensitivePersonalData",
        function ($scope, $http, $timeout, $state, $stateParams, $confirm, notify, hasWriteAccess, theSystem, regularSensitiveData, sensitivePersonalData) {

            $scope.hasWriteAccess = hasWriteAccess;
            $scope.system = theSystem;
            $scope.updateUrl = 'api/itsystem/' + theSystem.id;

            $scope.regularSensitiveData = regularSensitiveData;
            $scope.sensitivePersonalData = sensitivePersonalData;


            $scope.updateDataLevel = function (OptionId, Checked, optionType) {

                var msg = notify.addInfoMessage("Arbejder ...", false);

                if (Checked == true) {

                    var data = {
                        ObjectId: theSystem.id,
                        OptionId: OptionId,
                        OptionType: optionType
                    };

                    $http.post("Odata/AttachedOptions/", data, { handleBusy: true }).success(function (result) {
                        msg.toSuccessMessage("Felt Opdateret!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                
                } else {
                    $http.delete("Odata/RemoveOption(id=" + OptionId + ", systemId=" + theSystem.id + ",type='" + optionType + "')").success(function () {
                        msg.toSuccessMessage("Felt Opdateret!");
                    }).error(function () {
                        msg.toErrorMessage("Fejl!");
                    });
                }
            }

            $scope.changeDataField = function (fieldName) {
                var data;

                switch (fieldName) {
                    case 'containsLegalInfo':
                     data = {
                        ContainsLegalInfo : $scope.system.containsLegalInfo
                    };
                    break;
                    case 'isDataTransferedToThirdCountries':
                     data = {
                            IsDataTransferedToThirdCountries: $scope.system.isDataTransferedToThirdCountries
                        };
                    break;
                }

                $http.patch("api/itsystem/" + theSystem.id + "?organizationId=" + theSystem.organizationId, data).success(function (result) {

                    notify.addSuccessMessage("Success");

                }).error(function (result) {
                    notify.addErrorMessage('Fejl!');
                });
            };
        }]);
})(angular, app);
