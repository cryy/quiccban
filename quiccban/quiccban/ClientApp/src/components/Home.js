"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var Login_1 = require("./Login");
var core_1 = require("@material-ui/core");
function Home(props) {
    if (!props.user)
        return React.createElement(Login_1.default, null);
    else
        return React.createElement(core_1.Typography, null,
            "Hi, ",
            props.user.username);
}
exports.default = Home;
//# sourceMappingURL=Home.js.map