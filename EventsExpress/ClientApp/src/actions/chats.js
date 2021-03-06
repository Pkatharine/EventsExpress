import EventsExpressService from '../services/EventsExpressService';



export const GET_CHATS_PENDING = "GET_CHATS_PENDING";
export const GET_CHATS_SUCCESS = "GET_CHATS_SUCCESS";
export const GET_CHATS_ERROR = "GET_CHATS_ERROR";
export const GET_UNREAD_MESSAGES = "GET_UNREAD_MESSAGES";
export const RESET_NOTIFICATION = "RESET_NOTIFICATION";
const api_serv = new EventsExpressService();

export default function get_chats() {

    return dispatch => {
        dispatch(getChatsPending(true));

        const res = api_serv.getChats();
        console.log(res);
        res.then(response => {
            console.log(response);
            if (response.error == null) {

                dispatch(getChatsSuccess({isSuccess: true, data: response}));
            } else {
                dispatch(getChatsError(response.error));
            }
        });
    }
}
export function resetNotification(){
    return dispatch => 
    dispatch({type: RESET_NOTIFICATION });
}
export function getUnreadMessages(userId) {
    return dispatch => {
        console.log(userId);
    var res = api_serv.getUnreadMessages(userId);
    console.log(res);
    res.then(response => {
        console.log(response);
        if (response.error == null) {
            dispatch({type: GET_UNREAD_MESSAGES, payload: response});
        } 
    });
    }
}


export function getChatsSuccess(data) {
    return {
        type: GET_CHATS_SUCCESS,
        payload: data
    };
}

export function getChatsPending(data) {
    return {
        type: GET_CHATS_PENDING,
        payload: data
    };
}

export function getChatsError(data) {
    return {
        type: GET_CHATS_ERROR,
        payload: data
    };
}
