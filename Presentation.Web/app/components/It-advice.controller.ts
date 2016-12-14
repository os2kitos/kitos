(function (ng, app) {
    app.controller('object.EditAdviceCtrl', ['$','$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', '$uibModal','Roles','object','users','type',
        function ($,$scope, $http, $state, $stateParams, $timeout, notify, $modal,roles,object, users, type) {

            $scope.type = type;
            $scope.object = object;
            console.log($stateParams);

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    transport: {
                        read: {
                            url: `/Odata/advice?$filter=type eq '` + type + `' and RelationId eq ` + object.id + `&$expand=Reciepients, Advicesent`,
                            dataType: "json"
                        },
                    },
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                },
                selectable: true,
                change: onChange,
                    columns: [{
                        field: "SentDate",
                        title: "Sidst sendt"
                        },
                        {
                            field: "Id",
                            hidden: true
                        },
                        {
                            field: "Name",
                            title: "Navn"
                        },
                        {
                            field: "AlarmDate",
                            title: "Dato"
                        },
                        {
                            field: "Reciepients",
                            template: function (dataItem) {
                                var html = [];
                                for (var i = 0; i < dataItem.Reciepients.length; i++) {
                                    if (dataItem.Reciepients[i].RecpientType == 'RECIEVER'){ 
                                        html.push(dataItem.Reciepients[i].Name);
                                    }
                                }
                                return html.join(', ');
                            },
                            title: "Modtager"
                        },
                        {
                            field: "Id",
                            title: "CC",
                            template: function (dataItem) {
                                var html = [];
                                for (var i = 0; i < dataItem.Reciepients.length; i++) {
                                    if (dataItem.Reciepients[i].RecpientType == 'CC') {
                                        html.push(dataItem.Reciepients[i].Name);
                                    }
                                }
                                return html.join(', ');
                            }
                        },
                        {
                            field: "Subject",
                            title: "Emne"
                        }
                ],
                    toolbar: [
                        {
                            name: "opretRolle",
                            text: "Opret rolle",
                            template: "<button id=\"add-advice\" class=\"btn btn-success btn-sm\" data-ng-click=\"newAdvice('POST')\"><i class=\"glyphicon glyphicon-plus small\" > </i> Ny</button>"
                        }
                    ],
                pageable: {
                    refresh: true,
                    pageSizes: [10, 25, 50, 100, 200],
                    buttonCount: 5
                },
            };

            $scope.detailGridOptions = {
                dataSource: {
                type: "odata-v4",
                transport: {
                    read:  {
                        url: "/Odata/adviceSent?$filter=AdviceId eq 0",
                            dataType: "json"
                    },
                },
                pageSize: 10,
                serverPaging: true,
                serverFiltering: true,
            },
                columns: [{
                    field: "AdviceSentDate",
                    title: "Afsendt"
                }
                ],
            };

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            function onChange(arg) {

                var grid = $("#mainGrid").data("kendoGrid");
                var selectedItem = grid.dataItem(grid.select());
                console.log(selectedItem);
                $("#detailGrid").data("kendoGrid").dataSource.transport.options.read.url = "/Odata/adviceSent?$filter=AdviceId eq " + selectedItem.Id;
                $("#detailGrid").data("kendoGrid").dataSource.read();
            };

            $scope.newAdvice = function (action) {

                $scope.action = action;
                
                var modalInstance = $modal.open({
             
                windowClass: "modal fade in",
                templateUrl: "app/components/it-advice-modal-view.html",
                controller: ["$scope", "$uibModalInstance", "users", "Roles", "$window", "type", "action", "object", function ($scope, $modalInstance,users, roles, $window, type, action,object) {

                    //console.log($rootscope);
                    $scope.externalCC = "Rasmus@live.dk, ";
                    $scope.type = type;
                   // $scope.recieverUsers = users.data.value; For user mails in suggestions
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
                        var url = "Odata/advice";
                        var payload = createPayload();
                        //setup scheduling
                        payload.Scheduling = $scope.repitionPattern;
                        payload.AlarmDate = new Date($scope.formData.date);
                        payload.StopDate = new Date($scope.formData.stopDate);
                        httpCall(payload, action, url);
                    };

                    $scope.send = () => {
                        var url = "Odata/advice";
                        var payload = createPayload();
                        httpCall(payload,action,url);
                    };

                    function httpCall(payload, action, url) {
                        $http({
                            method: action,
                            url: url,
                            data: payload
                        }).success(function () {
                            //msg.toSuccessMessage("Ændringerne er gemt!");
                            $("#mainGrid").data("kendoGrid").dataSource.read();
                            $scope.$close(true);
                        }).error(function () {
                            //msg.toErrorMessage("Ændringerne kunne ikke gemmes!");
                        });
                    }

                    function createPayload() {

                        var payload = {
                            Subject: $scope.formData.subject,
                            Body: $scope.emailBody,
                            RelationId: object.id,
                            Type: type,
                            Scheduling: 'Immediate',
                            Reciepients: [],
                            AlarmDate: null,
                            StopDate: null

                        };

                        var writtenEmail = $scope.externalTo;
                        var writtenCCEmail = $scope.externalCC;

                        if ($scope.selectedRecievers != undefined) {
                            for (var i = 0; i < $scope.selectedRecievers.length; i++) {
                                payload.Reciepients.push(
                                    {
                                        Name: $scope.selectedRecievers[i],
                                        RecpientType: "ROLE",
                                        RecieverType: "RECIEVER"
                                    }
                                );
                            }
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
                        return payload;
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