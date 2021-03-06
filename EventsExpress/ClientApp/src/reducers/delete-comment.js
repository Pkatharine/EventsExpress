﻿import initialState from '../store/initialState';
import {
    SET_COMMENT_DELETE_PENDING,
    SET_COMMENT_DELETE_SUCCESS,
    SET_COMMENT_DELETE_ERROR
} from "../actions/delete-comment";

export const reducer = (state = initialState.delete_comment, action) => {

    switch (action.type) {
        case SET_COMMENT_DELETE_PENDING:
            return Object.assign({}, state, {
                isCommentDeletePending: action.isCommentDeletePending
            });

        case SET_COMMENT_DELETE_SUCCESS:
            return Object.assign({}, state, {
                isCommentDeleteSuccess: action.isCommentDeleteSuccess
            });

        case SET_COMMENT_DELETE_ERROR:
            return Object.assign({}, state, {
                commentDeleteError: action.commentDeleteError
            });

        default:
            return state;
    }
}