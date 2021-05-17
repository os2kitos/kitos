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
                        sort: {
                            field: "AlarmDate",
                            dir: "asc"
                        },
                        pageSize: 10,
                        serverPaging: true,
                        serverFiltering: true,
                        serverSorting: true
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
                            title: "Fra dato",
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
                            title: "Til dato",
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
                                var canDelete = dataItem.AdviceSent.length === 0;
                                if (hasWriteAccess) {
                                    return `<button class="btn-link" data-ng-click="newAdvice('PATCH',${dataItem.Id})">
                                    <i class="glyphicon glyphicon-pencil"></i></button>
                                    <button class="btn-link" ng-disabled="${!canDelete}" data-ng-click="deleteAdvice(${dataItem.Id})"><i class="glyphicon glyphicon-trash"></i></button>`;
                                } else {
                                    return "Ingen rettigheder";
                                }
                            }
                        }
                    ],
                    toolbar: [
                        {
                            name: "advis",
                            text: "Opret advis",
                            template:
                                "<button data-element-type=\"NewAdviceButton\" data-ng-disabled=\"!hasWriteAccess\" class=\"btn btn-success btn-sm\" data-ng-click=\"newAdvice('POST')\"><i class=\"glyphicon glyphicon-plus small\" ></i>Ny</button>"
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
                            "$scope", "Roles", "$window", "type", "action", "object", "currentUser", "entityMapper", "adviceData",
                            ($scope, roles, $window, type, action, object, currentUser: Kitos.Services.IUser, entityMapper: Kitos.Services.LocalOptions.IEntityMapper, adviceData) => {
                                $scope.showRoleFields = true;
                                $scope.hasWriteAccess = hasWriteAccess;
                                $scope.selectedReceivers = [];
                                $scope.selectedCCs = [];
                                $scope.adviceTypeData;
                                $scope.adviceRepetitionData;
                                $scope.adviceTypeOptions = Kitos.Models.ViewModel.Advice.AdviceTypeOptions.options;
                                $scope.adviceRepetitionOptions = Kitos.Models.ViewModel.Advice.AdviceRepetitionOptions.options;

                                $scope.multipleEmailValidationRegex = "(([a-zA-Z\\-0-9\\.]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)(, )*)+"

                                var select2Roles = entityMapper.mapRoleToSelect2ViewModel(roles);
                                if (select2Roles) {
                                    $scope.receiverRoles = select2Roles;
                                } else {
                                    $scope.showRoleFields = false;
                                }
                                if (action === "POST") {
                                    $scope.advisName = "Opret advis";
                                    $scope.hideSend = false;
                                    $scope.isActive = true;
                                    $scope.emailBody =
                                        `<a href='${$window.location.href.replace("advice", "main")}'>Link til ${type
                                        }</a>`;
                                }
                                if (action === "PATCH") {
                                    $scope.hideSend = true;
                                    $scope.advisName = "Redigere advis";
                                    if (id != undefined) {
                                        $scope.name = adviceData.Name;
                                        $scope.subject = adviceData.Subject;
                                        $scope.emailBody = adviceData.Body;
                                        $scope.adviceTypeData = Kitos.Models.ViewModel.Advice.AdviceTypeOptions.getOptionFromEnumString(adviceData.AdviceType);
                                        $scope.adviceRepetitionData = Kitos.Models.ViewModel.Advice.AdviceRepetitionOptions.getOptionFromEnumString(adviceData.Scheduling);
                                        $scope.startDate = adviceData.AlarmDate;
                                        $scope.stopDate = adviceData.StopDate;
                                        $scope.hiddenForjob = adviceData.JobId;
                                        $scope.isActive = adviceData.IsActive;
                                        $scope.preSelectedReceivers = [];
                                        $scope.preSelectedCCs = [];

                                        const recievers = [];
                                        const ccs = [];
                                        for (let i = 0; i < adviceData.Reciepients.length; i++) {
                                            let recpientType = adviceData.Reciepients[i].RecpientType;
                                            let recieverType = adviceData.Reciepients[i].RecieverType;
                                            if (recpientType === "ROLE" && recieverType === "RECIEVER") {
                                                var nameOfRoleReceiver = adviceData.Reciepients[i].Name;
                                                var selectedReceiver = _.find(select2Roles, x => x.text === nameOfRoleReceiver);
                                                if (selectedReceiver !== undefined) {
                                                    $scope.preSelectedReceivers.push(selectedReceiver);
                                                }
                                            } else if (recpientType === "ROLE" && recieverType === "CC") {
                                                var nameOfRoleCC = adviceData.Reciepients[i].Name;
                                                var selectedCC = _.find(select2Roles, x => x.text === nameOfRoleCC);
                                                if (selectedCC !== undefined) {
                                                    $scope.preSelectedCCs.push(selectedCC);
                                                }
                                            } else if (recpientType === "USER" && recieverType === "RECIEVER") {
                                                recievers.push(adviceData.Reciepients[i].Name);
                                            } else if (recpientType === "USER" &&
                                                recieverType === "CC") {
                                                ccs.push(adviceData.Reciepients[i].Name);
                                            }
                                        }
                                        $scope.externalTo = recievers.join(", ");
                                        $scope.externalCC = ccs.join(", ");
                                    }
                                }

                                $scope.save = () => {
                                    var url = "";
                                    var payload = createPayload();
                                    payload.Name = $scope.name;
                                    if ($scope.adviceTypeData.id === "1") {
                                    payload.Scheduling = $scope.adviceRepetitionData.id;
                                    payload.AlarmDate = dateString2Date($scope.startDate);
                                    payload.StopDate = dateString2Date($scope.stopDate);
                                    payload.StopDate.setHours(23, 59, 59, 99);
                                    }
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
                                
                                $scope.validateInputs = () => {

                                    if ($scope.adviceTypeData == null) {
                                        return true;
                                    }

                                    switch ($scope.adviceTypeData.id) {
                                        case "0":
                                            if (($scope.externalTo || $scope.selectedReceivers.length > 0) &&
                                                $scope.subject &&
                                                $scope.isEditable()) {
                                                return false;
                                            }
                                            else {
                                                return true;
                                            }
                                        case "1":
                                            if (($scope.externalTo || $scope.selectedReceivers.length > 0) &&
                                                $scope.subject &&
                                                $scope.adviceRepetitionData &&
                                                $scope.stopDate &&
                                                $scope.startDate &&
                                                $scope.checkErrStart($scope.startDate, $scope.stopDate) &&
                                                $scope.checkErrEnd($scope.startDate, $scope.stopDate) &&
                                                $scope.isEditable()) {
                                                return false;
                                            }
                                            else {
                                                return true;
                                            }
                                        default:
                                            return true;
                                    }
                                }

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
                                        Scheduling: "",
                                        AdviceType: $scope.adviceTypeData.id,
                                        Reciepients: [],
                                        AlarmDate: null,
                                        StopDate: null,
                                        JobId: $scope.hiddenForjob
                                    };

                                    const writtenEmail = $scope.externalTo;
                                    const writtenCCEmail = $scope.externalCC;

                                    if ($scope.selectedReceivers != undefined) {
                                        for (var i = 0; i < $scope.selectedReceivers.length; i++) {
                                            payload.Reciepients.push(
                                                {
                                                    Name: $scope.selectedReceivers[i].text,
                                                    RecpientType: "ROLE",
                                                    RecieverType: "RECIEVER"
                                                }
                                            );
                                        }
                                    }

                                    if ($scope.selectedCCs != undefined) {
                                        for (var i = 0; i < $scope.selectedCCs.length; i++) {
                                            payload.Reciepients.push(
                                                {
                                                    Name: $scope.selectedCCs[i].text,
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
                                }],
                            adviceData: ["$http", ($http: ng.IHttpService) => {
                                if (action === "PATCH") {
                                    return $http.get(`Odata/advice?key=${id}&$expand=Reciepients`)
                                        .then((res) => {
                                            if (res.status === 200) {
                                                return res.data;
                                            }
                                            return null;
                                        })
                                        .catch(_ => null);
                                }
                                return null;
                            }
                            ],
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