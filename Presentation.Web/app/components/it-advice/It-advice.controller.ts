(function (ng, app) {
    app.controller('object.EditAdviceCtrl', ['$', '$scope', '$http', '$state', '$stateParams', '$timeout', 'notify', '$uibModal', 'Roles', 'object', 'type', 'advicename', 'hasWriteAccess',
        function ($, $scope, $http, $state, $stateParams, $timeout, notify, $modal, roles, object, type, advicename, hasWriteAccess) {
            $scope.type = type;
            $scope.object = object;
            $scope.hasWriteAccess = hasWriteAccess;
            $scope.advicename = advicename;

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
                        var isActive = moment().isAfter(moment(dataItem.StopDate)) || dataItem.Scheduling === 'Immediate';
                        var canDelete = dataItem.AdviceSent.length === 0;
                        if (hasWriteAccess) {
                            return `<button class="btn-link" ng-disabled="${isActive}" data-ng-click="newAdvice('PATCH',${dataItem.Id})"><i class="glyphicon glyphicon-pencil"></i></button>
                                            <button class="btn-link" ng-disabled="${!canDelete}" data-ng-click="deleteAdvice(${dataItem.Id})"><i class="glyphicon glyphicon-trash"></i></button>`;
                        } else {
                            return 'Ingen rettigheder';
                        }
                    }
                }
                ],
                toolbar: [
                    {
                        name: "opretRolle",
                        text: "Opret rolle",
                        template: "<button data-ng-disabled=\"!hasWriteAccess\" class=\"btn btn-success btn-sm\" data-ng-click=\"newAdvice('POST')\"><i class=\"glyphicon glyphicon-plus small\" ></i>Ny</button>"
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
                        read: {
                            url: "/Odata/adviceSent?$filter=AdviceId eq 0",
                            dataType: "json"
                        },
                    },
                    pageSize: 25
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
                ],
                scrollable: {
                    virtual: true
                }
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

            var modalOpen = false;

            $scope.newAdvice = function (action, id) {

                $scope.hasWriteAccess = hasWriteAccess;
                $scope.action = action;

                if (modalOpen === false) {
                    modalOpen = true;
                    var modalInstance = $modal.open({

                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-advice/it-advice-modal-view.html",
                        controller: ["$scope", "$uibModalInstance", "Roles", "$window", "type", "action", "object", "currentUser", function ($scope, $modalInstance, roles, $window, type, action, object, currentUser) {

                            $scope.showRoleFields = true;
                            modalOpen = true;

                            if (roles.data) {
                                $scope.recieverRoles = roles.data.value;
                            } else {
                                $scope.showRoleFields = false;
                                $scope.collapsed = false;
                            }

                            if (action === 'POST') {
                                $scope.hideSend = false;
                                $scope.externalCC = currentUser.email;
                                $scope.emailBody = "<a href='" + $window.location.href.replace("advice", "main") + "'>" + "Link til " + type + "</a>";
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
                                        var ccs = [];
                                        $scope.selectedCC = [];

                                        for (var i = 0; i < response.data.Reciepients.length; i++) {
                                            if (response.data.Reciepients[i].RecpientType == 'ROLE' && response.data.Reciepients[i].RecieverType == 'RECIEVER') {
                                                $scope.selectedRecievers.push(response.data.Reciepients[i].Name);
                                            } else if (response.data.Reciepients[i].RecpientType == 'ROLE' && response.data.Reciepients[i].RecieverType == 'CC') {
                                                $scope.selectedCC.push(response.data.Reciepients[i].Name);
                                            } else if (response.data.Reciepients[i].RecpientType == 'USER' && response.data.Reciepients[i].RecieverType == 'RECIEVER') {
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
                                payload.StopDate.setHours(23, 59, 59, 99);

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

                                            $("#mainGrid").data("kendoGrid").dataSource.read();
                                            $scope.$close(true);
                                        },
                                        () => { () => { notify.addErrorMessage("Fejl! Kunne ikke opdatere modalen!") } }
                                        );

                                }

                            };

                            $scope.send = () => {
                                var url = "Odata/advice";
                                var payload = createPayload();
                                httpCall(payload, action, url);
                            };

                            $scope.checkErrStart = function (startDate, endDate) {
                                $scope.errMessage = '';
                                $scope.startDateErrMessage = '';
                                $scope.curDate = new Date();
                                if (!moment($scope.startDate, 'dd-MM-yyyy').isValid() || $scope.startDate == undefined) {
                                    $scope.startDateErrMessage = 'Fra Dato er ugyldig!';
                                    return false;
                                }
                                if ($scope.startDate && $scope.stopDate) {
                                    if ((dateString2Date($scope.startDate) > dateString2Date($scope.stopDate))) {
                                        $scope.errMessage = '\'Til Dato\' skal være senere end eller samme som \'Fra dato\'!';
                                        return false;
                                    }
                                } else {
                                    $scope.errMessage = 'Begge dato felter skal udfyldes!';
                                    return false;
                                }
                                
                                $scope.startDateErrMessage = '';
                                $scope.errMessage = '';
                                return true;
                            };
                            $scope.checkErrEnd = function (startDate, endDate) {
                                $scope.errMessage = '';
                                $scope.stopDateErrMessage = '';
                                $scope.curDate = new Date();
                                if (!moment($scope.stopDate, 'dd-MM-yyyy').isValid() || $scope.stopDate == undefined) {
                                    $scope.stopDateErrMessage = 'Til Dato er ugyldig!';
                                    return false;
                                }
                                if ($scope.startDate && $scope.stopDate) {
                                    if ((dateString2Date($scope.startDate) > dateString2Date($scope.stopDate))) {
                                        $scope.errMessage = '\'Til Dato\' skal være senere end eller samme som \'Fra dato\'!';
                                        return false;
                                    }
                                } else {
                                    $scope.errMessage = 'Begge dato felter skal udfyldes!';
                                    return false;
                                }
                                
                                $scope.stopDateErrMessage = '';
                                $scope.errMessage = '';
                                return true;
                            };

                            $scope.tinymceOptions = {
                                plugins: 'link image code',
                                skin: 'lightgray',
                                theme: 'modern',
                                toolbar: "bold italic | example | code | preview | link | searchreplace",
                                convert_urls: false
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
                                        $scope.$close(true);
                                        $("#mainGrid").data("kendoGrid").dataSource.read();
                                    }
                                    if (action === "PATCH") {
                                        notify.addSuccessMessage("Advisen er opdateret!");
                                    }
                                }).error(function () {
                                    if (action === "POST") {
                                        notify.addErrorMessage("Fejl! Kunne ikke oprette modalen!");
                                    }
                                    if (action === "PATCH") {
                                        notify.addErrorMessage("Fejl! Kunne ikke opdatere modalen!");
                                    }
                                })
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
                            modalOpen = false;
                        }],
                        resolve: {
                            Roles: ['$http', function ($http) {
                                if (type == "itSystemUsage") {
                                    return $http.get("odata/LocalItSystemRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                        .then(function (result) {
                                            return result;
                                        });
                                }
                                if (type == "itContract") {
                                    return $http.get("odata/LocalItContractRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                        .then(function (result) {
                                            return result;
                                        });
                                }
                                if (type == "itProject") {
                                    return $http.get("odata/LocalItProjectRoles?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc")
                                        .then(function (result) {
                                            return result;
                                        });
                                }
                                if (type == "itInterface") {
                                    return [];
                                }
                            }],
                            type: [function () {
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
                            ],
                            advicename: [() => {
                                return $scope.advicename;
                            }
                            ],
                            hasWriteAccess: [function () {
                                return $scope.hasWriteAccess;
                            }]
                        }
                    });
                }
            }
        }]);

})(angular, app);