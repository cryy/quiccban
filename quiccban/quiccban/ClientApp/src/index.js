"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var React = require("react");
var ReactDOM = require("react-dom");
var react_router_dom_1 = require("react-router-dom");
var App_1 = require("./App");
var serviceWorker = require("./registerServiceWorker");
var rootElement = document.getElementById('root');
ReactDOM.render(React.createElement(react_router_dom_1.BrowserRouter, null,
    React.createElement(App_1.default, null)), rootElement);
serviceWorker.register();
//# sourceMappingURL=index.js.map