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

                        var selectedRoles = $scope.selectedRecievers.split(','); //+ ", " + $scope.externalTo;
                        var selectedCCRoles = $scope.selectedCC.split(',');//$scope.externalCC + ", " +
                        var writtenEmail = $scope.externalTo.split(',');
                        var writtenCCEmail = $scope.externalCC.split(',');

                        for (var i = 0; i < selectedRoles.length; i++){
                            payload.Reciepients.push(
                                {
                                    Name: selectedRoles[i],
                                    RecpientType: "ROLE",
                                    RecieverType: "RECIEVER"
                                }
                            );
                        }
                        for (var i = 0; i < selectedCCRoles.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: selectedCCRoles[i],
                                    RecieverType: "CC",
                                    RecpientType: "ROLE"
                                }
                            );
                        }
                        for (var i = 0; i < writtenEmail.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: writtenEmail[i],
                                    RecpientType: "USER",
                                    RecieverType: "RECIEVER"
                                }
                            );
                        }
                        for (var i = 0; i < writtenCCEmail.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: writtenCCEmail[i],
                                    RecieverType: "CC",
                                    RecpientType: "USER"
                                }
                            );
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

                        console.log("ACTION: " + action);
                        console.log("Mail content");
                        console.log("selectedRecievers: " + $scope.selectedRecievers);
                        console.log("externalTo: " + $scope.externalTo);
                        console.log("selectedCC: " + $scope.selectedCC);
                        console.log("externalCC: " + $scope.externalCC);
                        console.log("gatherede reciepients: " + $scope.externalCC+", " + $scope.selectedCC+ ", " + $scope.selectedRecievers+", " + $scope.externalTo);
                        console.log("formData.subject: " + $scope.formData.subject);
                        console.log("emailBody: " + $scope.emailBody);
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