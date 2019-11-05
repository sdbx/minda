import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import Main from './pages/Main';

import 'tailwindcss/dist/base.css';
import 'tailwindcss/dist/components.css';
import 'tailwindcss/dist/utilities.css';

const App = (
    <BrowserRouter>
        <Switch>
            <Route exact path="/" component={Main} />
        </Switch>
    </BrowserRouter>
);

ReactDOM.render(App, document.getElementById('root'));
