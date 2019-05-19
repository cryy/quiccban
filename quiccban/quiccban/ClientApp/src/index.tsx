import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import * as serviceWorker from './registerServiceWorker';

const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter>
    <App />
  </BrowserRouter>,
  rootElement);

serviceWorker.register();
