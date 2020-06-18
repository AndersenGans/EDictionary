import { combineReducers } from "redux";
import { getWordsReducer, addWordReducer, updateWordReducer } from '../screens/dashboard/reducers/reducer';

export default combineReducers({
	getWordsReducer,
	addWordReducer,
	updateWordReducer
});