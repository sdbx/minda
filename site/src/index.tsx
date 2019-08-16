import React from 'react';
import ReactDOM from 'react-dom';
import Main from './pages/Main';
import { BrowserRouter, Route, Switch } from 'react-router-dom';

const App = (
    <BrowserRouter>
        <Switch>
            <Route exact={true} path={'/'} component={Main}/>
        </Switch>
    </BrowserRouter>
);

ReactDOM.render(App, document.getElementById('root'));