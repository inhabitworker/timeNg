var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var _this = this;
var hostPath = function () { return location.protocol + "//" + location.host; };
/// Config:
// fetch config and push variables into css -> more elegant than in app code, allows styling loading screen etc.
// fetch and set css from api
var getCss = function () { return __awaiter(_this, void 0, void 0, function () {
    var endpoint, css;
    return __generator(this, function (_a) {
        switch (_a.label) {
            case 0:
                endpoint = "/api/config/css";
                return [4 /*yield*/, fetch(hostPath() + endpoint)
                        .then(function (response) { return response.json(); })];
            case 1:
                css = _a.sent();
                setCss(css);
                return [2 /*return*/];
        }
    });
}); };
// fetch and set (dictionary of properties/value)
var setCss = function (cssProps) {
    // var root = document.getRootNode();
    var root = document.querySelector(":root");
    if (!(root instanceof HTMLElement))
        return;
    // let rootStyle = () => getComputedStyle(root);
    // for init
    root.style.setProperty("visibility", "visible");
    for (var key in cssProps) {
        var value = cssProps[key];
        root.style.setProperty(key, value);
    }
};
/// UI
// stuff for ui
// disable default behaviour within special text box used inside form for tag chooser
var disableSubmit = function () {
    var inputters = window.document.getElementsByClassName("inputTagText");
    Array.prototype.forEach.call(inputters, function (el) { return el.onkeypress = function (e) {
        if (e.keyCode == 13) {
            e.preventDefault();
        }
    }; });
};
/// Active interval ticking:
// I was previously doing this as a blazor server app and this seemed to make more sense
// maybe less so now but even still probably nicer than disposing timer etc
// format single digit hrs to leading 0
function leading(input) {
    if (input / 10 < 1) {
        return "0".concat(input);
    }
    return input.toString();
}
;
// keep active times ticking
var elapsed = function (start) {
    var elapsedElements = window.document.getElementsByClassName("elapsedActive");
    var startDate = new Date(start);
    var time = new Date();
    var duration = new Date(time.getTime() - startDate.getTime());
    var hours = Math.floor(duration.getTime() / (1000 * 60 * 60));
    var newString = leading(hours) + ":" + leading(duration.getUTCMinutes()) + ":" + leading(duration.getUTCSeconds());
    console.log(elapsedElements);
    Array.prototype.forEach.call(elapsedElements, function (e) { return e.innerHTML = newString; });
    setTimeout(elapsed, 1000, start);
};
var downloadFileFromStream = function (fileName, contentStreamReference) { return __awaiter(_this, void 0, void 0, function () {
    var arrayBuffer, blob, url, anchorElement;
    return __generator(this, function (_a) {
        switch (_a.label) {
            case 0: return [4 /*yield*/, contentStreamReference.arrayBuffer()];
            case 1:
                arrayBuffer = _a.sent();
                blob = new Blob([arrayBuffer]);
                url = URL.createObjectURL(blob);
                anchorElement = document.createElement('a');
                anchorElement.href = url;
                anchorElement.download = fileName !== null && fileName !== void 0 ? fileName : '';
                anchorElement.click();
                anchorElement.remove();
                URL.revokeObjectURL(url);
                return [2 /*return*/];
        }
    });
}); };
//# sourceMappingURL=app.js.map