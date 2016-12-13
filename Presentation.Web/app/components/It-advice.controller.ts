(function (ng, app) {
    app.controller('object.EditAdviceCtrl', ['$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', '$uibModal','Roles','object','users','type',
        function ($scope, $http, $state, $stateParams, $timeout, notify, $modal,roles,object, users, type) {

            $scope.type = type;
            $scope.object = object;

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };
            
            $scope.newAdvice = function (action) {

                $scope.action = action;
                
                var modalInstance = $modal.open({

                windowClass: "modal fade in",
                templateUrl: "app/components/it-advice-modal-view.html",
                controller: ["$scope", "$uibModalInstance","users","Roles","$window","type","action","object", function ($scope, $modalInstance,users, roles, $window, type, action,object) {
                   
                    $scope.type = type;
                   // $scope.recieverUsers = users.data.value;
                    $scope.recieverRoles = roles.data.value;

                    $scope.tinymceOptions = {
                        plugins: 'link image code',
                        skin: 'lightgray',
                        theme: 'modern',
                        toolbar: "bold italic | example | code | preview | link | searchreplace"
                    };

                    $scope.emailBody = "<a href='" + $window.location.href.replace("advice/" + type, "main") +"'>"+"Link til " + type +"</a>";

                    $scope.datepickerOptions = {
                        format: "dd-MM-yyyy",
                        parseFormats: ["yyyy-MM-dd"]
                    };

                    $scope.save = () => {

                    };

                    
                    $scope.send = () => {

                        var payload = {
                            Subject: $scope.formData.subject,
                            Body: $scope.emailBody,
                            RelationId: object.id,
                            Type: type,
                            Scheduling: 'Immediate',
                            Reciepients: []
                        };

                        var url = "Odata/advice";

                       /* var selectedRoles = $scope.selectedRecievers; //+ ", " + $scope.externalTo;
                        var selectedCCRoles = $scope.selectedCC;//$scope.externalCC + ", " +*/
                        var writtenEmail = $scope.externalTo;
                        var writtenCCEmail = $scope.externalCC;


                        for (var i = 0; i < $scope.selectedRecievers.length; i++){
                            payload.Reciepients.push(
                                {
                                    Name: $scope.selectedRecievers[i],
                                    RecpientType: "ROLE",
                                    RecieverType: "RECIEVER"
                                }
                            );
                        }

                        if ($scope.selectedCC != undefined) {
                        for (var i = 0; i < $scope.selectedCC.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: $scope.selectedCC[i],
                                    RecieverType: "CC",
                                    RecpientType: "ROLE"
                                }
                            );
                            }
                        }
                        if (writtenEmail != undefined) {
                            for (var i = 0; i < writtenEmail.split(',').length; i++) {
                                payload.Reciepients.push(
                                    {
                                        Name: writtenEmail.split(',')[i],
                                        RecpientType: "USER",
                                        RecieverType: "RECIEVER"
                                    }
                                );
                            }
                        }
                        if (writtenCCEmail != undefined) {
                            for (var i = 0; i < writtenCCEmail.split(',').length; i++) {
                                payload.Reciepients.push(
                                    {
                                        Name: writtenCCEmail.split(',')[i],
                                        RecieverType: "CC",
                                        RecpientType: "USER"
                                    }
                                );
                            }
                        }
                        

                        if (action === 'PATCH') {
                            url = "";
                        }

                        $http({
                            method: action,
                            url: url,
                            data: payload
                        }).success(function () {
                            //msg.toSuccessMessage("Ændringerne er gemt!");
                            $scope.$close(true);
                        }).error(function () {
                            //msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                            });
                    };
                }],
                resolve: {
                    Roles: ['$http', function ($http) {
                        return $http.get("odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                            .then(function (result) {
                                return result;
                            });
                    }],
                    advices: ['$http', '$stateParams', function ($http, $stateParams) {
                        return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                            return result.data.response.advices;
                        });
                    }],
                    users: ['UserGetService', function (UserGetService) {
                        return UserGetService.GetAllUsers();
                    }],
                    type: [ function () {
                        return $scope.type;
                    }],
                    action: [function () {
                        return $scope.action;
                    }],
                    object: [function () {
                        return $scope.object;
                    }]

                }
            });
        }
        }]);

})(angular, app);