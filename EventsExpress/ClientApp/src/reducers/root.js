import React from 'react';
import { combineReducers } from 'redux';
import { routerReducer, routerMiddleware } from 'react-router-redux';
import {reducer as LoginReducer} from './login';
import { reducer as formReducer } from 'redux-form';
import * as User from './user';
import * as Register from './register';
import * as AddEvent from './add-event';
import * as Events from './event-list';
import * as Countries from './countries';
import * as Cities from './cities';

const rootReducers = {
    user: User.reducer,
    routing: routerReducer,
    form: formReducer,
    login: LoginReducer,
    register: Register.reducer,
    add_event: AddEvent.reducer,
    events: Events.reducer,
    countries: Countries.reducer,
    cities: Cities.reducer
};



export default rootReducers;