import React from 'react';
import ReactDOM from 'react-dom';
import Main from './pages/Main';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import Stat from './pages/Stat';

const App = (
    <BrowserRouter>
        <Switch>
            <Route exact={true} path={'/'} component={Main}/>
            <Route exact={true} path={'/stat'} component={Stat}/>
        </Switch>
    </BrowserRouter>
);

ReactDOM.render(App, document.getElementById('root'));