import { all, takeEvery, put, call } from 'redux-saga/effects';
import { Routine } from 'redux-saga-routines';
import { fetchWordsRoutine, addWordRoutine, updateWordRoutine } from '../routines';
import * as wordsService from '../services/words.service';

function* fetchWords ({ payload }: Routine<any>) {
	try {
		yield call(wordsService.getWords);
		yield put(fetchWordsRoutine.success(payload));
	} catch (error) {
		yield put(fetchWordsRoutine.failure(error));
	}
}

function* addWord ({ payload }: Routine<any>) {
	try {
		yield call(wordsService.addWord, payload);
		yield put(addWordRoutine.success(payload));
	} catch (error) {
		yield put(addWordRoutine.failure(error));
	}
}

function* updateWord ({ payload }: Routine<any>) {
	try {
		yield call(wordsService.updateWord, payload);
		yield put(updateWordRoutine.success(payload));
	} catch (error) {
		yield put(updateWordRoutine.failure(error));
	}
}

function* watchFetchWords() {
	yield takeEvery(fetchWordsRoutine.TRIGGER, fetchWords);
}

function* watchAddWord() {
	yield takeEvery(addWordRoutine.TRIGGER, addWord);
}

function* watchUpdateWord() {
	yield takeEvery(updateWordRoutine.TRIGGER, updateWord);
}

export function* wordsSagas() {
	yield all([
		watchFetchWords(),
		watchAddWord(),
		watchUpdateWord()
	]);
  }