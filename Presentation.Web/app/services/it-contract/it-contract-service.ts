module Kitos.Services.ItContract {

    export interface IItContractService {
        getValidationDetails(contractId: number): ng.IPromise<Kitos.Models.ItContract.IItContractValidationDetailsResponseDTO>;
    }

    export class ItContractService implements IItContractService {
        static $inject = ["$http"];
        constructor(private readonly $http: ng.IHttpService) { }

        getValidationDetails(contractId: number): ng.IPromise<Kitos.Models.ItContract.IItContractValidationDetailsResponseDTO> {
            return this.$http
                .get<API.Models.IApiWrapper<Kitos.Models.ItContract.IItContractValidationDetailsResponseDTO>>("api/itcontract/" + contractId + "/validation-details")
                .then(response => {
                    return response.data.response;
                });
        }
    }

    app.service("itContractService", ItContractService);
}