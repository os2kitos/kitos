var app = angular.module("reportApp", [
    "ui.router",
    "ui.bootstrap",
    "ngAnimate",
    "ngSanitize",
    "notify"]);

app.constant("$", $)
   .constant("_", _);