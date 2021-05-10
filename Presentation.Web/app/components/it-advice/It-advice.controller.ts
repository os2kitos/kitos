((ng, app) => {
    app.controller("object.EditAdviceCtrl",
        [
            "$", "$scope", "$http", "notify", "$uibModal", "object", "type", "advicename", "hasWriteAccess",
            ($, $scope, $http, notify, $modal, object, type, advicename, hasWriteAccess) => {
                $scope.type = type;
                $scope.object = object;
                $scope.advicename = advicename;

                $scope.mainGridOptions = {
                    dataSource: {
                        type: "odata-v4",
                        transport: {
                            read: {
                                url: `/Odata/advice?$filter=type eq '${type}' and RelationId eq ${object.id
                                    }&$expand=Reciepients, Advicesent`,
                                dataType: "json"
                            },
                        },
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                    },
                    selectable: true,
                    change: onChange,
                    columns: [
                        {
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
                            field: "Reciepients.Name",
                            title: "Modtager",
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
                                var isActive = dataItem.isActive || dataItem.Scheduling === "Immediate";
                                var canDelete = dataItem.AdviceSent.length === 0;
                                if (hasWriteAccess) {
                                    return `<button class="btn-link" ng-disabled="${isActive
                                        }" data-ng-click="newAdvice('PATCH',${dataItem.Id
                                        })"><i class="glyphicon glyphicon-pencil"></i></button>
                                    <button class="btn-link" ng-disabled="${!canDelete}" data-ng-click="deleteAdvice(${
                                        dataItem.Id})"><i class="glyphicon glyphicon-trash"></i></button>`;
                                } else {
                                    return "Ingen rettigheder";
                                }
                            }
                        }
                    ],
                    toolbar: [
                        {
                            name: "opretRolle",
                            text: "Opret rolle",
                            template:
                                "<button data-ng-disabled=\"!hasWriteAccess\" class=\"btn btn-success btn-sm\" data-ng-click=\"newAdvice('POST')\"><i class=\"glyphicon glyphicon-plus small\" ></i>Ny</button>"
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
                    columns: [
                        {
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

                $scope.deleteAdvice = (id) => {
                    $http.delete(`odata/advice(${id})`)
                        .then(() => {
                                notify.addSuccessMessage("Advisen er slettet!");
                                $("#mainGrid").data("kendoGrid").dataSource.read();
                            },
                            () => notify.addErrorMessage("Fejl! Kunne ikke slette!"));
                };

                $scope.newAdvice = (action, id) => {
                    $scope.hasWriteAccess = hasWriteAccess;
                    $scope.action = action;
                    var modalInstance = $modal.open({
                        windowClass: "modal fade in",
                        templateUrl: "app/components/it-advice/it-advice-modal-view.html",
                        backdrop: "static",
                        controller: [
                            "$scope", "Roles", "$window", "type", "action", "object", "currentUser", "hasWriteAccess",
                            ($scope, roles, $window, type, action, object, currentUser: Kitos.Services.IUser, hasWriteAccess: boolean) => {
                                $scope.showRoleFields = true;
                                $scope.collapsed = true;
                                $scope.CCcollapsed = true;
                                $scope.hasWriteAccess = hasWriteAccess;
                                if (roles) {
                                    $scope.recieverRoles = roles;
                                } else {
                                    $scope.showRoleFields = false;
                                }
                                if (action === "POST") {
                                    $scope.hideSend = false;
                                    $scope.externalCC = currentUser.email;
                                    $scope.isActive = true;
                                    $scope.emailBody =
                                        `<a href='${$window.location.href.replace("advice", "main")}'>Link til ${type
                                        }</a>`;
                                }
                                if (action === "PATCH") {
                                    $scope.hideSend = true;
                                    if (id != undefined) {
                                        $http({
                                            method: "GET",
                                            url: `Odata/advice?key=${id}&$expand=Reciepients`
                                        }).then(function successCallback(response) {
                                                $scope.name = response.data.Name;
                                                $scope.subject = response.data.Subject;
                                                $scope.emailBody = response.data.Body;
                                                $scope.repitionPattern = response.data.Scheduling;
                                                $scope.startDate = response.data.AlarmDate;
                                                $scope.stopDate = response.data.StopDate;
                                                $scope.hiddenForjob = response.data.JobId;
                                                $scope.isActive = response.data.IsActive;
                                                $scope.selectedRecievers = [];
                                                $scope.selectedCC = [];
                                                const ccs = [];
                                                for (let i = 0; i < response.data.Reciepients.length; i++) {
                                                    let recpientType = response.data.Reciepients[i].RecpientType;
                                                    let recieverType = response.data.Reciepients[i].RecieverType;
                                                    if (recpientType === "ROLE" && recieverType === "RECIEVER") {
                                                        $scope.selectedRecievers.push(response.data.Reciepients[i].name);
                                                    } else if (recpientType === "ROLE" && recieverType === "CC") {
                                                        $scope.selectedCC.push(response.data.Reciepients[i].Name);
                                                    } else if (recpientType === "USER" && recieverType === "RECIEVER") {
                                                        $scope.externalTo = response.data.Reciepients[i].Name;
                                                    } else if (recpientType === "USER" &&
                                                        recieverType === "CC") {
                                                        ccs.push(response.data.Reciepients[i].Name);
                                                    }
                                                }
                                                $scope.externalCC = ccs.join(", ");
                                            },
                                            function errorCallback(response) {
                                            });
                                    }
                                }

                                $scope.save = () => {
                                    var url = "";
                                    var payload = createPayload();
                                    payload.Name = $scope.name;
                                    payload.Scheduling = $scope.repitionPattern;
                                    payload.AlarmDate = dateString2Date($scope.startDate);
                                    payload.StopDate = dateString2Date($scope.stopDate);
                                    payload.StopDate.setHours(23, 59, 59, 99);
                                    if (action === "POST") {
                                        url = `Odata/advice?organizationId=${currentUser.currentOrganizationId}`;
                                        httpCall(payload, action, url);
                                    } else if (action === "PATCH") {
                                        // 2021-05-09 mhs: Move this downwards (KITOSUDV-1673)
                                        url = `Odata/advice(${id})`;
                                        $http.delete(`/api/AdviceUserRelation/DeleteByAdviceId?adviceId=${id}`);
                                        for (let i = 0; i < payload.Reciepients.length; i++) {
                                            payload.Reciepients[i].adviceId = id;
                                            $http.post(`/api/AdviceUserRelation?organizationId=${currentUser
                                                .currentOrganizationId}`,
                                                payload.Reciepients[i]);
                                        }
                                        payload.Reciepients = undefined;
                                        $http.patch(url, JSON.stringify(payload))
                                            .then(() => {
                                                    notify.addSuccessMessage("Advisen er opdateret!");
                                                    $("#mainGrid").data("kendoGrid").dataSource.read();
                                                    $scope.$close(true);
                                                },
                                                () => {
                                                    () => {
                                                        notify.addErrorMessage("Fejl! Kunne ikke opdatere modalen!")
                                                    }
                                                }
                                            );
                                    }
                                };

                                $scope.send = () => {
                                    var url = `Odata/advice?organizationId=${currentUser.currentOrganizationId}`;
                                    var payload = createPayload();
                                    httpCall(payload, action, url);
                                };

                                $scope.deactivate = () => {
                                    if ($scope.isActive) {
                                        const url = `Odata/DeactivateAdvice?key=${id}`;
                                        $http.patch(url)
                                            .then(() => {
                                                notify.addSuccessMessage("Advisen er opdateret!");
                                                $("#mainGrid").data("kendoGrid").dataSource.read();
                                                $scope.$close(true);
                                            });
                                    }
                                };

                                $scope.isEditable = () => {
                                    return $scope.hasWriteAccess && $scope.isActive;
                                };

                                $scope.checkErrStart = (startDate, endDate) => {
                                    $scope.errMessage = "";
                                    $scope.startDateErrMessage = "";
                                    $scope.curDate = new Date();
                                    if (!moment($scope.startDate, "dd-MM-yyyy").isValid() ||
                                        $scope.startDate == undefined) {
                                        $scope.startDateErrMessage = "Fra Dato er ugyldig!";
                                        return false;
                                    }
                                    if ($scope.startDate && $scope.stopDate) {
                                        if ((dateString2Date($scope.startDate) > dateString2Date($scope.stopDate))) {
                                            $scope.errMessage =
                                                "'Til Dato' skal være senere end eller samme som 'Fra dato'!";
                                            return false;
                                        }
                                    } else {
                                        $scope.errMessage = "Begge dato felter skal udfyldes!";
                                        return false;
                                    }

                                    $scope.startDateErrMessage = "";
                                    $scope.errMessage = "";
                                    return true;
                                };

                                $scope.checkErrEnd = (startDate, endDate) => {
                                    $scope.errMessage = "";
                                    $scope.stopDateErrMessage = "";
                                    $scope.curDate = new Date();
                                    if (!moment($scope.stopDate, "dd-MM-yyyy").isValid() ||
                                        $scope.stopDate == undefined) {
                                        $scope.stopDateErrMessage = "Til Dato er ugyldig!";
                                        return false;
                                    }
                                    if ($scope.startDate && $scope.stopDate) {
                                        if ((dateString2Date($scope.startDate) > dateString2Date($scope.stopDate))) {
                                            $scope.errMessage =
                                                "'Til Dato' skal være senere end eller samme som 'Fra dato'!";
                                            return false;
                                        }
                                    } else {
                                        $scope.errMessage = "Begge dato felter skal udfyldes!";
                                        return false;
                                    }

                                    $scope.stopDateErrMessage = "";
                                    $scope.errMessage = "";
                                    return true;
                                };

                                $scope.tinymceOptions = {
                                    plugins: "link image code",
                                    skin: "lightgray",
                                    theme: "modern",
                                    toolbar: "bold italic | example | code | preview | link | searchreplace",
                                    convert_urls: false
                                };

                                $scope.datepickerOptions = {
                                    format: "dd-MM-yyyy",
                                    parseFormats: ["yyyy-MM-dd"]
                                };

                                function dateString2Date(dateString) {
                                    const dt = dateString.split("-");
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
                                    }).then(function onSuccess(result) {
                                            if (action === "POST") {
                                                notify.addSuccessMessage("Advisen er oprettet!");
                                                $scope.$close(true);
                                                $("#mainGrid").data("kendoGrid").dataSource.read();
                                            }
                                            if (action === "PATCH") {
                                                notify.addSuccessMessage("Advisen er opdateret!");
                                            }
                                        },
                                        function onError(result) {
                                            if (action === "POST") {
                                                notify.addErrorMessage("Fejl! Kunne ikke oprette advis!");
                                            }
                                            if (action === "PATCH") {
                                                notify.addErrorMessage("Fejl! Kunne ikke opdatere advis!");
                                            }
                                        });
                                }

                                function createPayload() {
                                    const payload = {
                                        Name: "Straks afsendt",
                                        Subject: $scope.subject,
                                        Body: $scope.emailBody,
                                        RelationId: object.id,
                                        Type: type,
                                        Scheduling: "Immediate",
                                        Reciepients: [],
                                        AlarmDate: null,
                                        StopDate: null,
                                        JobId: $scope.hiddenForjob
                                    };

                                    const writtenEmail = $scope.externalTo;
                                    const writtenCCEmail = $scope.externalCC;

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
                                        for (var i = 0; i < writtenEmail.split(",").length; i++) {
                                            payload.Reciepients.push(
                                                {
                                                    Name: writtenEmail.split(",")[i],
                                                    RecpientType: "USER",
                                                    RecieverType: "RECIEVER"
                                                }
                                            );
                                        }
                                    }
                                    if (writtenCCEmail != undefined) {
                                        for (var i = 0; i < writtenCCEmail.split(",").length; i++) {
                                            payload.Reciepients.push(
                                                {
                                                    Name: writtenCCEmail.split(",")[i],
                                                    RecieverType: "CC",
                                                    RecpientType: "USER"
                                                }
                                            );
                                        }
                                    }
                                    return payload;
                                };
                            }
                        ],
                        resolve: {
                            Roles: [
                                "localOptionServiceFactory",
                                (localOptionServiceFactory: Kitos.Services.LocalOptions.ILocalOptionServiceFactory) => {
                                    if (type === "itSystemUsage") {
                                        return localOptionServiceFactory
                                            .create(Kitos.Services.LocalOptions.LocalOptionType.ItSystemRoles).getAll();
                                    }
                                    if (type === "itContract") {
                                        return localOptionServiceFactory
                                            .create(Kitos.Services.LocalOptions.LocalOptionType.ItContractRoles)
                                            .getAll();
                                    }
                                    if (type === "itProject") {
                                        return localOptionServiceFactory
                                            .create(Kitos.Services.LocalOptions.LocalOptionType.ItProjectRoles)
                                            .getAll();
                                    }
                                    if (type === "dataProcessingRegistration") {
                                        return localOptionServiceFactory.create(Kitos.Services.LocalOptions
                                            .LocalOptionType.DataProcessingRegistrationRoles).getAll();
                                    }
                                    if (type === "itInterface") {
                                        return [];
                                    }
                                }
                            ],
                            type: [() => $scope.type],
                            action: [() => $scope.action],
                            object: [() => $scope.object],
                            currentUser: [
                                "userService",
                                (userService: Kitos.Services.IUserService) => userService.getUser()
                            ],
                            advicename: [
                                () => {
                                    return $scope.advicename;
                                }
                            ],
                            hasWriteAccess: [
                                () => {
                                    return $scope.hasWriteAccess;
                                }
                            ]
                        }
                    });
                    modalInstance.result.then(angular.noop, angular.noop);
                };

                function onChange(arg) {
                    const grid = $("#mainGrid").data("kendoGrid");
                    const selectedItem = grid.dataItem(grid.select());
                    $("#detailGrid").data("kendoGrid").dataSource.transport.options.read.url =
                        `/Odata/adviceSent?$filter=AdviceId eq ${selectedItem.Id}`;
                    $("#detailGrid").data("kendoGrid").dataSource.read();
                };
            }
        ]);
})(angular, app);