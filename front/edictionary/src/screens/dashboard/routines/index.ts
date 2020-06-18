import { createRoutine } from 'redux-saga-routines';

export const fetchWordsRoutine =  createRoutine('GET_WORDS');
export const addWordRoutine =  createRoutine('ADD_WORD');
export const updateWordRoutine =  createRoutine('UPDATE_WORD');
