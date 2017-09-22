﻿module Kitos.Services {
    export class UploadFileService {

        public static $inject: string[] = ["$http", "notify"];

        constructor(private $http: IHttpServiceWithCustomConfig, private notify) {
        }

        public uploadFile(file: File) {

            var url = "/api/uploadFile";
            var fd = new FormData();
            fd.append('kontraktIndgåelse', file);

            var msg = this.notify.addSuccessMessage("Uploader fil...");

            this.$http.post(url, fd, {
                transformRequest: angular.identity,
                headers: { 'Content-Type': undefined }
            })
                .success(function () {
                    msg.toSuccessMessage("fil uploaded!");
                })
                .error(function () {
                    msg.toErrorMessage("Fejl! Fil kunne ikke uploades!");
                });
        }
    }
    app.service("uploadFile", UploadFileService);
}
