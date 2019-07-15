"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var core_1 = require("@material-ui/core");
var useStyle = core_1.makeStyles(function (theme) {
    var _a;
    return core_1.createStyles({
        title: (_a = {
                fontWeight: 200,
                fontSize: "42px"
            },
            _a[theme.breakpoints.down('sm')] = {
                fontSize: "32px"
            },
            _a.textAlign = "center",
            _a),
        divider: {
            marginTop: "3px",
            marginBottom: "4px"
        },
        gridItem: {
            textAlign: "center",
            top: "50%",
            left: "50%",
            position: "absolute",
            transform: 'translate(-50%, -50%)'
        }
    });
});
function Login() {
    var classes = useStyle();
    return (React.createElement(core_1.Grid, { container: true, justify: "center", alignItems: "center", direction: "column", spacing: 0 },
        React.createElement(core_1.Grid, { item: true, xs: 3, className: classes.gridItem },
            React.createElement(core_1.Typography, { className: classes.title }, "quiccban web UI"),
            React.createElement(core_1.Divider, { className: classes.divider }),
            React.createElement(core_1.Button, { variant: "outlined", color: "primary", href: "/api/auth/login" }, "Login with Discord"))));
}
exports.default = Login;
//# sourceMappingURL=Login.js.map