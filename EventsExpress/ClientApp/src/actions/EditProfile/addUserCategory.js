﻿import EventsExpressService from '../../services/EventsExpressService';
import { SetAlert} from '../alert';
export const addUserCategory = {
    PENDING: "SET_ADDUSERCATEGORY_PENDING",
    SUCCESS: "SET_ADDUSERCATEGORY_SUCCESS",
    ERROR: "SET_ADDUSERCATEGORY_ERROR",
    UPDATE: "UPDATE_CATEGORIES"
}

const api_serv = new EventsExpressService();

export default function add_UserCategory(data) {
    return dispatch => {
        dispatch(setAddUserCategoryPending(true));
        const res = api_serv.setUserCategory(data);
        res.then(response => {
            if (response.error == null) {

                dispatch(setAddUserCategorySuccess(true));
                dispatch(updateCategories(data));
                dispatch(SetAlert({variant:'success', message:'Favarote categoris is updated'}));
            } else {
                dispatch(setAddUserCategoryError(response.error));
                dispatch(SetAlert({variant:'error', message:'Failed'}));
            }
        });

    }
}

function updateCategories(data) {
    return {
        type: addUserCategory.UPDATE,
        payload: data
    };
}

function setAddUserCategoryPending(data) {
    return {
        type: addUserCategory.PENDING,
        payload: data
    };
}

function setAddUserCategorySuccess(data) {
    return {
        type: addUserCategory.SUCCESS,
        payload: data
    };
}

export function setAddUserCategoryError(data) {
    return {
        type: addUserCategory.ERROR,
        payload: data
    };
}

