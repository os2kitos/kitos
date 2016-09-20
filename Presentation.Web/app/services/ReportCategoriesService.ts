module Kitos.Services {
    export class ReportCategoriesService {

        static $inject = ["$http"];
        private baseUrl = "/odata/ReportCategories";

         constructor(private $http: ng.IHttpService) {
        }

        GetById = (id: number) => {
            return this.$http.get<Models.IOptionEntity>(`${this.baseUrl}(${id})`);
        }

        GetAll = () => {
            return this.$http.get<Kitos.Models.IODataResult<Models.IOptionEntity>>(this.baseUrl);
        }
    }

    app.service("reportCategoriesService", ReportCategoriesService);
}
