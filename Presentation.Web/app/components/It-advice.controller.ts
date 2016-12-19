(function (ng, app) {
    app.controller('object.EditAdviceCtrl', ['$', '$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', '$uibModal', 'Roles', 'object', 'users', 'type', 
        function ($, $scope, $http, $state, $stateParams, $timeout, notify, $modal, roles, object, users, type) {
            $scope.type = type;
            $scope.object = object;

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
                    field: "Name",
                    title: "Navn",
                    attributes: { "class": "might-overflow" }
                },
                {
                            field: "SentDate",
                            title: "Sidst sendt",
                            template: x => {
                                if (x.SentDate != null) {
                                    return kendo.toString(new Date(x.SentDate), "d");
                                }
                                return "";
                            },
                            attributes: { "class": "might-overflow" }
                        },
                        {
                            field: "Id",
                            hidden: true
                        },
                        
                        {
                            field: "AlarmDate",
                            title: "Start dato",
                            template: x => {
                                if (x.AlarmDate != null) {
                                    return kendo.toString(new Date(x.AlarmDate), "d");
                                }
                                return "";
                            },
                            attributes: { "class": "might-overflow" }
                        },
                        {
                            field: "StopDate",
                            title: "Slut dato",
                            template: x => {
                                if (x.StopDate != null) {
                                    return kendo.toString(new Date(x.StopDate), "d");
                                }
                                return "";
                            },
                            attributes: { "class": "might-overflow" }
                        },
                        {
                            field: "Reciepients.Name", title: "Modtager",
                            template: () =>
                                `<span data-ng-model="dataItem.Reciepients" value="cc.Name" ng-repeat="cc in dataItem.Reciepients | filter: { RecieverType: 'RECIEVER'}"> {{cc.Name}}{{$last ? '' : ', '}}</span>`,
                            attributes: { "class": "might-overflow" }
                        },
                        {
                            field: "Reciepients.Name",
                            title: "CC",
                            template: () =>
                                `<span data-ng-model="dataItem.Reciepients" value="cc.Name" ng-repeat="cc in dataItem.Reciepients | filter: { RecieverType: 'CC'}"> {{cc.Name}}{{$last ? '' : ', '}}</span>`,
                            attributes: { "class": "might-overflow" }
                        },
                        {
                            field: "Subject",
                            title: "Emne"
                        },
                        {
                            template: (dataItem) => {
                                var isActive = (kendo.toString(new Date(dataItem.StopDate), "d") < kendo.toString(new Date(), "d") || dataItem.Scheduling === 'Immediate');
                                return `<button id="add-advice" ng-show="${isActive}" ng-disabled="${isActive}" class="glyphicon glyphicon-ban-circle" data-ng-click="newAdvice('PATCH',${dataItem.Id})"></button>
                                                        <button id="add-advice" ng-hide="${isActive}" ng-disabled="${isActive}" class="glyphicon glyphicon-pencil" data-ng-click="newAdvice('PATCH',${dataItem.Id})"></button> ` +
                                    `<button id="add-advice" class="glyphicon glyphicon-trash" data-ng-click="deleteAdvice(${dataItem.Id})"></button>`;
                            }
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
                serverFiltering: true
                },
                columns: [{
                    field: "AdviceSentDate",
                    title: "Afsendt",
                    template: x => {
                        if (x.AdviceSentDate != null) {
                            return kendo.toString(new Date(x.AdviceSentDate), "g");
                        }
                        return "";
                    }
                }
                ]
            };

            $scope.datepickerOptions = {
                format: "dd-MM-yyyy",
                parseFormats: ["yyyy-MM-dd"]
            };

            function onChange(arg) {

                var grid = $("#mainGrid").data("kendoGrid");
                var selectedItem = grid.dataItem(grid.select());
                $("#detailGrid").data("kendoGrid").dataSource.transport.options.read.url = "/Odata/adviceSent?$filter=AdviceId eq " + selectedItem.Id;
                $("#detailGrid").data("kendoGrid").dataSource.read();
            };

            $scope.deleteAdvice = (id) => {
                $http.delete(`odata/advice(${id})`)
                    .then(() => {
                            notify.addSuccessMessage("Advisen er slettet!");
                            $("#mainGrid").data("kendoGrid").dataSource.read();
                        },
                    () => notify.addErrorMessage("Fejl! Kunne ikke slette!"));
            }
            $scope.newAdvice = function (action, id) {

                $scope.action = action;
                var modalInstance = $modal.open({

                    windowClass: "modal fade in",
                    templateUrl: "app/components/it-advice-modal-view.html",
                    controller: ["$scope", "$uibModalInstance", "users", "Roles", "$window", "type", "action", "object", "currentUser", function ($scope, $modalInstance, users, roles, $window, type, action, object, currentUser) {

                        $scope.recieverRoles = roles.data.value;

                        if (action === 'POST') {
                            $scope.hideSend = false;
                            $scope.externalCC = currentUser.email;
                            $scope.emailBody = "<a href='" + $window.location.href.replace("advice/" + type, "main") + "'>" + "Link til " + type + "</a>";
                        }

                        if (action === 'PATCH') {
                            $scope.hideSend = true;

                            if (id != undefined) {

                                $http({
                                    method: 'GET',
                                    url: 'Odata/advice?key=' + id + '&$expand=Reciepients'
                                }).then(function successCallback(response) {

                                    $scope.name = response.data.Name;
                                    $scope.subject = response.data.Subject;
                                    $scope.emailBody = response.data.Body;
                                    $scope.repitionPattern = response.data.Scheduling;
                                    $scope.startDate = response.data.AlarmDate;
                                    $scope.stopDate = response.data.StopDate;
                                    $scope.selectedRecievers = [];
                                    $scope.hiddenForjob = response.data.JobId;
                                    //var recivers = [];
                                    var ccs = [];
                                    $scope.selectedCC = [];

                                    for (var i = 0; i < response.data.Reciepients.length; i++) {
                                        if (response.data.Reciepients[i].RecpientType == 'ROLE' && response.data.Reciepients[i].RecieverType == 'RECIEVER') {
                                            $scope.selectedRecievers.push(response.data.Reciepients[i].Name);
                                        } else if (response.data.Reciepients[i].RecpientType == 'ROLE' && response.data.Reciepients[i].RecieverType == 'CC') {
                                            $scope.selectedCC.push(response.data.Reciepients[i].Name);
                                        } else if (response.data.Reciepients[i].RecpientType == 'USER' && response.data.Reciepients[i].RecieverType == 'RECIEVER') {
                                            // recivers.push(response.data.Reciepients[i].Name);
                                            $scope.externalTo = response.data.Reciepients[i].Name;
                                        } else if (response.data.Reciepients[i].RecpientType == 'USER' && response.data.Reciepients[i].RecieverType == 'CC') {
                                            ccs.push(response.data.Reciepients[i].Name);
                                        }
                                    }
                                    $scope.externalCC = ccs.join(', ');
                                }, function errorCallback(response) {
                                });
                            }
                        }

                        $scope.save = () => {

                            var url = '';
                            var payload = createPayload();
                            //setup scheduling
                            payload.Name = $scope.name;
                            payload.Scheduling = $scope.repitionPattern;
                            payload.AlarmDate = dateString2Date($scope.startDate);
                            payload.StopDate = dateString2Date($scope.stopDate);

                            if (action == 'POST') {
                                url = "Odata/advice";

                                httpCall(payload, action, url);
                            } else if (action == 'PATCH') {

                                url = "Odata/advice(" + id + ")";

                                $http.delete('/api/AdviceUserRelation/DeleteByAdviceId?adviceId=' + id);

                                for (var i = 0; i < payload.Reciepients.length; i++) {
                                    payload.Reciepients[i].adviceId = id;
                                    $http.post('/api/AdviceUserRelation', payload.Reciepients[i]);
                                }
                                payload.Reciepients = undefined;
                                $http.patch(url, JSON.stringify(payload))
                                    .then(() => {
                                        notify.addSuccessMessage("Advisen er opdateret!");
                                    },
                                    () => { () => { notify.addErrorMessage("Fejl! Kunne ikke opdatere modalen!") }}
                                );

                                $("#mainGrid").data("kendoGrid").dataSource.read();
                                $scope.$close(true);
                            }

                        };

                        $scope.send = () => {
                            var url = "Odata/advice";
                            var payload = createPayload();
                            httpCall(payload, action, url);
                        };

                        $scope.tinymceOptions = {
                            plugins: 'link image code',
                            skin: 'lightgray',
                            theme: 'modern',
                            toolbar: "bold italic | example | code | preview | link | searchreplace"
                        };

                        $scope.datepickerOptions = {
                            format: "dd-MM-yyyy",
                            parseFormats: ["yyyy-MM-dd"]
                        };
                        function dateString2Date(dateString) {
                            var dt = dateString.split('-');
                            if (dt[2].length > 4) {
                                return new Date(dt[0] + "/" + dt[1] + "/" + dt[2].substring(0, 2));
                        }
                        return new Date(dt[2] + "/" + dt[1] + "/" + dt[0].substring(0, 2));
                        }

                    function httpCall(payload, action, url) {
                        $http({
                            method: action,
                            url: url,
                            data: payload,
                            type: "application/json"
                        }).success(function () {
                            if (action === "POST") {
                                notify.addSuccessMessage("Advisen er oprettet!");
                            }
                            if (action === "PATCH") {
                                notify.addSuccessMessage("Advisen er opdateret!");
                            }
                            $("#mainGrid").data("kendoGrid").dataSource.read();
                            $scope.$close(true);
                        }).error(function () {
                            if (action === "POST") {
                                notify.addErrorMessage("Fejl! Kunne ikke oprette modalen!");
                            }
                            if (action === "PATCH") {
                                notify.addErrorMessage("Fejl! Kunne ikke opdatere modalen!");
                            }
                        });
                    }

                    function createPayload() {

                        var payload = {
                            Name: "Straks afsendt",
                            Subject: $scope.subject,
                            Body: $scope.emailBody,
                            RelationId: object.id,
                            Type: type,
                            Scheduling: 'Immediate',
                            Reciepients: [],
                            AlarmDate: null,
                            StopDate: null,
                            JobId: $scope.hiddenForjob
                        };

                        var writtenEmail = $scope.externalTo;
                        var writtenCCEmail = $scope.externalCC;

                        if ($scope.selectedRecievers != undefined) {
                            for (var i = 0; i < $scope.selectedRecievers.length; i++) {
                                payload.Reciepients.push(
                                    {
                                        Name: $scope.selectedRecievers[i],
                                        RecpientType: "ROLE",
                                        RecieverType: "RECIEVER",
                                        adviceId: undefined
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
                        if (type === "itSystemUsage"|| "itInterface") {
                            return $http.get("odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                .then(function (result) {
                                    return result;
                                });
                        }
                        if (type === "itContract") {
                            return $http.get("odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                .then(function (result) {
                                    return result;
                                });
                        }
                        if (type === "itProject") {
                            return $http.get("odata/LocalItProjectRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                .then(function (result) {
                                    return result;
                                });
                        }
                    }],
                    advices: ['$http', '$stateParams', function ($http, $stateParams) {
                      
                        switch ($scope.type) { 
                            case 'itContract':
                                return $http.get('api/itcontract/' + $stateParams.id).then(function (result) {
                                    return result.data.response.advices;
                                });
                            case 'ItSystemUsage':
                                return $http.get('api/ItSystemUsage/' + $stateParams.id).then(function (result) {
                                    return result.data.response.advices;
                                });
                            case 'itproject':
                                return $http.get('api/itproject/' + $stateParams.id).then(function (result) {
                                    return result.data.response.advices;
                                });
                            case 'itInterface':
                                return $http.get('api/itInterface/' + $stateParams.id).then(function (result) {
                                    return result.data.response.advices;
                                });
                    }
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
                    }],
                    currentUser: ["userService",
                        (userService) => userService.getUser()
                    ]
                }
            });
        }
        }]);

})(angular, app);