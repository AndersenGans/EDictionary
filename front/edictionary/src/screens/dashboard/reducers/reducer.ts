import { IWord } from "../models/word";
import { Routine } from "redux-saga-routines";
import { fetchWordsRoutine, addWordRoutine, updateWordRoutine } from "../routines";

export interface IWordState {
    words: IWord[]
}

export const getWordsReducer = (state: IWordState = { words: [] }, action: Routine<any>): IWordState => {
    const { type, payload } = action;

    switch (type) {
        case fetchWordsRoutine.SUCCESS:
            return { ...state, words: payload };
        default:
            return state;
    }
}

export const addWordReducer = (state: IWordState = { words: [] }, action: Routine<any>): IWordState => {
    const { type, payload } = action;
    const { words } = state;

    switch (type) {
        case addWordRoutine.SUCCESS:
            return { ...state, words: words.concat(payload) };
        default:
            return state;
    }
}

export const updateWordReducer = (state: IWordState = { words: [] }, action: Routine<any>): IWordState => {
    const { type, payload } = action;
    const { words } = state;

    switch (type) {
        case updateWordRoutine.SUCCESS:
            return { ...state, words: words.map( (item) => item.id === payload.id ? payload : item ) };
        default:
            return state;
    }
}