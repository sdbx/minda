import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import Main from './pages/Main';

const App = (
    <BrowserRouter>
        <Switch>
            <Route exact path="/" component={Main} />
        </Switch>
    </BrowserRouter>
);

ReactDOM.render(App, document.getElementById('root'));
