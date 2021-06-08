module Kitos.ItAdvice.Modal.Create {
    "use strict";

    export function createModalInstance(_, $, $modal, $scope, notify, $http, type, action, id, hasWriteAccess) {
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
                    currentUser: Services.IUser,
                    entityMapper: Services.LocalOptions.IEntityMapper,
                    adviceData) => {
                    $scope.hasWriteAccess = hasWriteAccess;
                    $scope.selectedReceivers = [];
                    $scope.selectedCCs = [];
                    $scope.adviceTypeData = null;
                    $scope.adviceRepetitionData = null;
                    $scope.adviceTypeOptions = Models.ViewModel.Advice.AdviceTypeOptions.options;
                    $scope.adviceRepetitionOptions = Models.ViewModel.Advice.AdviceRepetitionOptions.options;
                    $scope.startDateInfoMessage = null;

                    //Format {email1},{email2}. Space between , and {email2} is ok but not required
                    const emailMatchRegex = "([a-zA-Z\\-0-9\\._]+@)([a-zA-Z\\-0-9\\.]+)\\.([a-zA-Z\\-0-9\\.]+)";
                    $scope.multipleEmailValidationRegex = `^(${emailMatchRegex}(((,)( )*)${emailMatchRegex})*)$`;

                    var payloadDateFormat = "YYYY-MM-DD";
                    var allowedDateFormats = [Constants.DateFormat.DanishDateFormat, payloadDateFormat];

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
                            $scope.startDate = adviceData.AlarmDate && convertDateTimeStringToDateString(adviceData.AlarmDate);
                            $scope.stopDate = adviceData.StopDate && convertDateTimeStringToDateString(adviceData.StopDate);
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
                            $scope.externalTo = receivers.length === 0 ? null : receivers.join(", ");
                            $scope.externalCC = ccs.length === 0 ? null : ccs.join(", ");
                        }
                    }

                    $scope.save = () => {
                        var url = "";
                        var payload = createPayload();
                        if (isCurrentAdviceRecurring()) {
                            payload.Name = $scope.name;
                            payload.Scheduling = $scope.adviceRepetitionData.id;
                            payload.AlarmDate = moment($scope.startDate, allowedDateFormats, true).format(payloadDateFormat);

                            //Stopdate is optional so only parse it if present
                            payload.StopDate = $scope.stopDate ? moment($scope.stopDate, allowedDateFormats, true).format(payloadDateFormat) : null;
                        }
                        if (action === "POST") {
                            url = `Odata/advice?organizationId=${currentUser.currentOrganizationId}`;
                            httpCall(payload, action, url);

                        } else if (action === "PATCH") {
                            url = `Odata/advice(${id})`;
                            // HACK: Reintroducing frontend logic for maintaining AdviceUserRelation -- Microsoft implementation of Odata PATCH flawed
                            patchAdviceUserRelation(id, payload)
                                .then(result => {
                                    delete payload.Reciepients;
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
                                });
                        }
                    };

                    function patchAdviceUserRelation(adviceId, payload) {
                        return payload.Reciepients.reduce((previousPromise, recipient) => {
                            recipient.adviceId = adviceId;
                            return previousPromise.then(() => $http.post(`/api/AdviceUserRelation?organizationId=${currentUser.currentOrganizationId}`, recipient));
                        },
                            $http.delete(`/api/AdviceUserRelation/DeleteByAdviceId?adviceId=${adviceId}`)
                        );
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

                    function isEditable(context: string) {
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
                    }

                    $scope.isEditable = (context = "") => {
                        return isEditable(context);
                    };

                    $scope.checkDates = (startDate, endDate) => {
                        $scope.startDateErrMessage = "";
                        $scope.stopDateErrMessage = "";
                        $scope.startDateInfoMessage = null;

                        const performStartDateValidation = isEditable("StartDate");
                        const performStopDateValidation = isEditable("StopDate");

                        if (!$scope.startDate) {
                            return false;
                        }

                        var start = moment($scope.startDate, allowedDateFormats, true);
                        if (performStartDateValidation) {
                            if (!start.isValid()) {
                                $scope.startDateErrMessage = "'Fra dato' er ugyldig!";
                                return false;
                            }

                            if (moment().isAfter(start, 'day') && action === "POST") {
                                $scope.startDateErrMessage = "'Fra dato' må ikke være før idag!";
                                return false;
                            }
                        }

                        if ($scope.stopDate && performStopDateValidation) {

                            var stop = moment($scope.stopDate, allowedDateFormats, true);

                            if (!stop.isValid()) {
                                $scope.stopDateErrMessage = "'Til dato' er ugyldig!";
                                return false;
                            }

                            if (moment().isAfter(stop, 'day')) {
                                $scope.stopDateErrMessage = "'Til dato' må ikke være før idag!";
                                return false;
                            }

                            if (start.isAfter(stop)) {
                                $scope.stopDateErrMessage = "'Til dato' skal være samme eller senere end 'Fra dato'!";
                                return false;
                            }

                        }

                        const repetition = $scope.adviceRepetitionData;
                        const dayInMonth = parseInt(start.format("DD"));
                        const month = parseInt(start.format("MM"));
                        var showIntervalWarning = false;
                        if (isCurrentAdviceRecurring() && repetition && dayInMonth > 28) {
                            switch (repetition.id) {
                                case Models.ViewModel.Advice.AdviceRepetition.Year:
                                    showIntervalWarning = month === 2; //Start data on feb 29 in leap year? then we show the warning even for annual repetition
                                    break;
                                case Models.ViewModel.Advice.AdviceRepetition.Quarter:
                                    showIntervalWarning =
                                        dayInMonth === 31 ||
                                        month % 3 ===
                                        2; //February is in the interval OR 31 is selected as start date (then months with 30 will be hit -> show the warning)
                                    break;
                                case Models.ViewModel.Advice.AdviceRepetition.Month:
                                    showIntervalWarning = true; //Always relevant for monthly intervals
                                    break;
                                case Models.ViewModel.Advice.AdviceRepetition.Semiannual:
                                    showIntervalWarning = dayInMonth === 31 || month % 6 === 2; //February is in the interval
                                    break;
                                default:
                                    showIntervalWarning = false;
                                    break;
                            }
                        }

                        if (showIntervalWarning) {
                            $scope.startDateInfoMessage =
                                "OBS: Du har valgt en 'Fra dato' større end 28 og et gentagelsesinterval der kan ramme måneder hvor dagen ikke findes. Hvis dagen ikke findes i måneden, vil advis blive afsendt den sidste dag i den aktuelle måned.";
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

                        for (let i = 0; i < $scope.selectedReceivers.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: $scope.selectedReceivers[i].text,
                                    RecpientType: "ROLE",
                                    RecieverType: "RECIEVER"
                                }
                            );
                        }

                        for (let i = 0; i < $scope.selectedCCs.length; i++) {
                            payload.Reciepients.push(
                                {
                                    Name: $scope.selectedCCs[i].text,
                                    RecieverType: "CC",
                                    RecpientType: "ROLE"
                                }
                            );
                        }

                        if (writtenEmail != null) {
                            const writtenToEmails = writtenEmail.split(",");
                            for (let i = 0; i < writtenToEmails.length; i++) {
                                const toEmail = writtenToEmails[i].trim();//Remove leading and trailing whitespace
                                if (toEmail && toEmail.length > 0) {
                                    payload.Reciepients.push(
                                        {
                                            Name: toEmail,
                                            RecpientType: "USER",
                                            RecieverType: "RECIEVER"
                                        }
                                    );
                                }
                            }
                        }
                        if (writtenCCEmail != null) {
                            const writtenCCEmails = writtenCCEmail.split(",");
                            for (let i = 0; i < writtenCCEmails.length; i++) {
                                const ccEmail = writtenCCEmails[i].trim();//Remove leading and trailing whitespace
                                if (ccEmail && ccEmail.length > 0) {
                                    payload.Reciepients.push(
                                        {
                                            Name: ccEmail,
                                            RecieverType: "CC",
                                            RecpientType: "USER"
                                        }
                                    );
                                }
                            }
                        }
                        return payload;
                    };

                    function convertDateTimeStringToDateString(dateTime: string): string {
                        return dateTime.substring(0, 10); //Take only the Date part of the DateTime string
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