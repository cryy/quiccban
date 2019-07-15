"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var core_1 = require("@material-ui/core");
var useStyle = core_1.makeStyles(function (theme) {
    return core_1.createStyles({
        loader: {
            position: "absolute",
            marginLeft: "50%",
            left: "-80px",
            top: "50%",
            marginTop: "-80px"
        }
    });
});
function Preloader() {
    var classes = useStyle();
    return React.createElement(core_1.CircularProgress, { className: classes.loader, size: 180, thickness: 1.2 });
}
exports.default = Preloader;
//# sourceMappingURL=Preloader.js.map