module Kitos.ItAdvice.Modal.Create {
    "use strict";

    export function createModalInstance(_, $, $modal, $scope, notify, $http, type, action, id, hasWriteAccess) {
        var allowedDateFormats = ["DD-MM-YYYY", "YYYY-MM-DDTHH:mm:ssZ", "YYYY-MM-DDTHH:mm:ss.SSSZ"];
        
        return $modal.open({
            windowClass: "modal fade in",
            templateUrl: "app/components/it-advice/it-advice-modal-view.html",
            backdrop: "static",
            controller: [
                "$scope", "Roles", "$window", "type", "action", "object", "currentUser", "entityMapper",
                "adviceData",
                ($scope,
                    roles,
                    $window,
                    type,
                    action,
                    object,
                    currentUser: Kitos.Services.IUser,
                    entityMapper: Kitos.Services.LocalOptions.IEntityMapper,
                    adviceData) => {
                    $scope.showRoleFields = true;
                    $scope.hasWriteAccess = hasWriteAccess;
                    $scope.selectedReceivers = [];
                    $scope.selectedCCs = [];
                    $scope.adviceTypeData = null; 
                    $scope.adviceRepetitionData;
                    $scope.adviceTypeOptions = Kitos.Models.ViewModel.Advice.AdviceTypeOptions.options;
                    $scope.adviceRepetitionOptions = Kitos.Models.ViewModel.Advice.AdviceRepetitionOptions.options;

                    $scope.multipleEmailValidationRegex = "(([a-zA-Z\\-0-9\\.]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)(, )*)+"

                    var allowedDateFormats = ["DD-MM-YYYY", "YYYY-MM-DDTHH:mm:ssZ", "YYYY-MM-DDTHH:mm:ss.SSSZ"];

                    var select2Roles = entityMapper.mapRoleToSelect2ViewModel(roles);
                    if (select2Roles) {
                        $scope.receiverRoles = select2Roles;
                    } else {
                        $scope.showRoleFields = false;
                    }
                    if (action === "POST") {
                        $scope.showDeactivate = false;
                        $scope.advisName = "Opret advis";
                        $scope.isActive = true;
                        $scope.emailBody =
                            `<a href='${$window.location.href.replace("advice", "main")}'>Link til ${type
                            }</a>`;
                    }
                    if (action === "PATCH") {
                        $scope.newAdvice = false;
                        $scope.showDeactivate = true;
                        $scope.advisName = "Rediger advis";
                        if (id != undefined) {
                            $scope.name = adviceData.Name;
                            $scope.subject = adviceData.Subject;
                            $scope.emailBody = adviceData.Body;
                            $scope.adviceTypeData = Models.ViewModel.Advice.AdviceTypeOptions.getOptionFromEnumString(adviceData.AdviceType);
                            $scope.adviceRepetitionData = Models.ViewModel.Advice.AdviceRepetitionOptions.getOptionFromEnumString(adviceData.Scheduling);
                            $scope.startDate = adviceData.AlarmDate;
                            $scope.stopDate = adviceData.StopDate;
                            $scope.hiddenForjob = adviceData.JobId;
                            $scope.isActive = adviceData.IsActive;
                            $scope.preSelectedReceivers = [];
                            $scope.preSelectedCCs = [];

                            const receivers = [];
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
                                    receivers.push(adviceData.Reciepients[i].Name);
                                } else if (recpientType === "USER" && recieverType === "CC") {
                                    ccs.push(adviceData.Reciepients[i].Name);
                                }
                            }
                            $scope.externalTo = receivers.length === 0 ? undefined : receivers.join(", ");
                            $scope.externalCC = ccs.length === 0 ? undefined : ccs.join(", ");
                        }
                    }

                    $scope.save = () => {
                        var url = "";
                        var payload = createPayload();
                        payload.Name = $scope.name;
                        if (isCurrentAdviceRecurring()) {
                            payload.Scheduling = $scope.adviceRepetitionData.id;
                            payload.AlarmDate = dateString2Date($scope.startDate);
                            payload.StopDate = dateString2Date($scope.stopDate);
                            payload.StopDate.setHours(23, 59, 59, 99);
                        }
                        if (action === "POST") {
                            url = `Odata/advice?organizationId=${currentUser.currentOrganizationId}`;
                            httpCall(payload, action, url);

                        } else if (action === "PATCH") {
                            url = `Odata/advice(${id})`;
                            // HACK: Reintroducing frontend logic for maintaining AdviceUserRelation -- Microsoft implementation of Odata PATCH flawed
                            patchAdviceUserRelation(id, payload);
                            payload.Reciepients = undefined;
                            $http.patch(url, JSON.stringify(payload))
                                .then(
                                    () => {
                                        notify.addSuccessMessage("Advisen er opdateret!");
                                        $("#mainGrid").data("kendoGrid").dataSource.read();
                                        $scope.$close(true);
                                    }, 
                                    () => {
                                        notify.addErrorMessage("Fejl! Kunne ikke opdatere advisen!");
                                    }
                                );
                        }
                    };

                    function patchAdviceUserRelation(id, payload) {
                        $http.delete('/api/AdviceUserRelation/DeleteByAdviceId?adviceId=' + id);
                        for (var i = 0; i < payload.Reciepients.length; i++) {
                            payload.Reciepients[i].adviceId = id;
                            $http.post(`/api/AdviceUserRelation?organizationId=${currentUser.currentOrganizationId}`, payload.Reciepients[i]);
                        }
                    }

                    function isCurrentAdviceImmediate() {
                        return $scope.adviceTypeData.id === "0";
                    }

                    function isCurrentAdviceRecurring() {
                        return $scope.adviceTypeData.id === "1";
                    }

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

                    $scope.isEditable = (context = "") => {
                        var editableInGeneral = $scope.hasWriteAccess && $scope.isActive;
                        if (editableInGeneral && action === "PATCH" && isCurrentAdviceRecurring()) {
                            if (context === "Name" || 
                                context === "Subject" || 
                                context === "StopDate" || 
                                context === "Deactivate" ||
                                context === "ToEmail" ||
                                context === "ToRole" ||
                                context === "CcEmail" ||
                                context === "CcRole" ||
                                context === "Save") {
                                return true;
                            }
                            return false;
                        }
                        return editableInGeneral;
                    };

                    $scope.checkDates = (startDate, endDate) => {
                        $scope.startDateErrMessage = "";
                        $scope.stopDateErrMessage = "";
                        $scope.curDate = new Date();

                        if ($scope.startDate === undefined) {
                            $scope.startDateErrMessage = "Fra Dato er ugyldig!";
                            return false;
                        }

                        var start = moment($scope.startDate, allowedDateFormats, true);

                        if (!start.isValid()) {
                            $scope.startDateErrMessage = "Fra Dato er ugyldig!";
                            return false;
                        }

                        if ($scope.stopDate !== undefined) {

                            var stop = moment($scope.stopDate, allowedDateFormats, true);

                            if (!stop.isValid()) {
                                $scope.stopDateErrMessage = "Til Dato er ugyldig!";
                                return false;
                            }

                            if (start.isAfter(stop)) {
                                $scope.stopDateErrMessage = "Til Dato skal være samme eller senere end Fra dato!";
                                return false;
                            }

                        }

                        $scope.startDateErrMessage = "";
                        $scope.stopDateErrMessage = "";
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
                    
                    $scope.hasInputErrors = () => {
                        if ($scope.adviceTypeData == null) {
                            return true;
                        }

                        if (isCurrentAdviceImmediate()) {
                            if (($scope.externalTo || $scope.selectedReceivers.length > 0) &&
                                $scope.subject &&
                                $scope.isEditable()) {
                                return false;
                            }
                            else {
                                return true;
                            }
                        }
                        if (isCurrentAdviceRecurring()) {
                            if (($scope.externalTo || $scope.selectedReceivers.length > 0) &&
                                $scope.subject &&
                                $scope.adviceRepetitionData &&
                                $scope.checkDates($scope.startDate, $scope.stopDate) &&
                                $scope.isEditable('Deactivate')) {
                                return false;
                            }
                            else {
                                return true;
                            }
                        }
                        return true;
                    }

                    function dateString2Date(dateString) {
                        const dt = dateString.split("-");
                        if (dt[2].length > 4) {
                            return new Date(dt[0] + "/" + dt[1] + "/" + dt[2].substring(0, 2)).toLocaleDateString(
                                'en-GB');

                        }
                        return new Date(dt[2] + "/" + dt[1] + "/" + dt[0].substring(0, 2)).toLocaleDateString('en-GB');
                    }

                    function getCurrentDate() {
                        return new Date().toLocaleDateString('en-GB');
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
                            IsActive: $scope.isActive,
                            Name: "Straks afsendt",
                            Subject: $scope.subject,
                            Body: $scope.emailBody,
                            RelationId: object.id,
                            Type: type,
                            Scheduling: null,
                            AdviceType: $scope.adviceTypeData.id,
                            Reciepients: [],
                            AlarmDate: null,
                            StopDate: null,
                            JobId: $scope.hiddenForjob
                        };

                        const writtenEmail = $scope.externalTo;
                        const writtenCCEmail = $scope.externalCC;

                        if ($scope.selectedReceivers != undefined) {
                            for (let i = 0; i < $scope.selectedReceivers.length; i++) {
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
                            for (let i = 0; i < $scope.selectedCCs.length; i++) {
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
                            for (let i = 0; i < writtenEmail.split(",").length; i++) {
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
                            for (let i = 0; i < writtenCCEmail.split(",").length; i++) {
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
                adviceData: [
                    "$http", ($http: ng.IHttpService) => {
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
    }
}