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

String.prototype.repeat = function (count) {
    if (count < 1) return '';
    var result = '', pattern = this.valueOf();
    while (count > 1) {
        if (count & 1) result += pattern;
        count >>= 1, pattern += pattern;
    }
    return result + pattern;
};
