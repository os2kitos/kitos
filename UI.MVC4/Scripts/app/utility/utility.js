function waitCursor() {
    $("body").addClass("wait");
}

function normalCursor() {
    $("body").removeClass("wait");
}

Array.prototype.pushArray = function (arr) {
    this.push.apply(this, arr);
};