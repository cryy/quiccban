"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var styles_1 = require("@material-ui/core/styles");
var AppBar_1 = require("@material-ui/core/AppBar");
var Toolbar_1 = require("@material-ui/core/Toolbar");
var Typography_1 = require("@material-ui/core/Typography");
var IconButton_1 = require("@material-ui/core/IconButton");
var Menu_1 = require("@material-ui/icons/Menu");
var MenuItem_1 = require("@material-ui/core/MenuItem");
var Menu_2 = require("@material-ui/core/Menu");
var core_1 = require("@material-ui/core");
var useStyles = styles_1.makeStyles(function (theme) {
    return styles_1.createStyles({
        root: {
            flexGrow: 1,
        },
        menuButton: {
            marginRight: theme.spacing(2),
        },
        title: {
            flexGrow: 1,
        },
        appBar: {
            backgroundColor: theme.palette.background.paper,
            position: "relative",
            top: "0%",
            left: "0%"
        },
    });
});
function Navbar(props) {
    var classes = useStyles();
    var _a = React.useState(null), anchorEl = _a[0], setAnchorEl = _a[1];
    var open = Boolean(anchorEl);
    function handleMenu(event) {
        setAnchorEl(event.currentTarget);
    }
    function handleClose() {
        setAnchorEl(null);
    }
    if (props.user) {
        return (React.createElement("div", { className: classes.root },
            React.createElement(AppBar_1.default, { elevation: 0, classes: { root: classes.appBar }, color: "default" },
                React.createElement(Toolbar_1.default, null,
                    React.createElement(IconButton_1.default, { edge: "start", className: classes.menuButton, color: "inherit", "aria-label": "Menu" },
                        React.createElement(Menu_1.default, null)),
                    React.createElement(Typography_1.default, { variant: "h6", className: classes.title }, "quiccban"),
                    React.createElement("div", null,
                        React.createElement(IconButton_1.default, { "aria-label": "Account of current user", "aria-controls": "menu-appbar", "aria-haspopup": "true", onClick: handleMenu, color: "inherit" },
                            React.createElement(core_1.Avatar, { src: "https://cdn.discordapp.com/avatars/" + props.user.id + "/" + props.user.avatarHash + ".webp?size=128", style: {
                                    width: "45px",
                                    height: "45px",
                                } })),
                        React.createElement(Menu_2.default, { id: "menu-appbar", anchorEl: anchorEl, getContentAnchorEl: null, anchorOrigin: { vertical: "bottom", horizontal: "center" }, transformOrigin: { vertical: "top", horizontal: "center" }, open: open, onClose: handleClose },
                            React.createElement(Typography_1.default, null,
                                props.user.username,
                                "#",
                                props.user.discriminator),
                            React.createElement(core_1.Divider, null),
                            React.createElement(MenuItem_1.default, { onClick: function (e) { return window.location.href = "/api/auth/logout"; } }, "Logout")))))));
    }
    else
        return React.createElement("div", null);
}
exports.default = Navbar;
//# sourceMappingURL=Navbar.js.map