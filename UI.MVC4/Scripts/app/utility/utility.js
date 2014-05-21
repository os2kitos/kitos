function waitCursor() {
    $("body").addClass("wait");
}

function normalCursor() {
    $("body").removeClass("wait");
}

// http://stackoverflow.com/questions/4156101/javascript-push-array-values-into-another-array
Array.prototype.pushArray = function (arr) {
    this.push.apply(this, arr);
};