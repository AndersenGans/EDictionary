import { wordsSagas } from "../screens/dashboard/sagas/sagas";
import { all } from "redux-saga/effects";

export default function* rootSaga() {
	yield all([
		wordsSagas(),
	]);
}