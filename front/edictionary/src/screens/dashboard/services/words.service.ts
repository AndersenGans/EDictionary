import { callWebApi } from "../../../services/api.service";
import { IWord } from "../models/word";

export const getWords = async () => callWebApi({
	endpoint: `/api/words`,
	type: 'GET'
});

export const addWord = async (word: IWord) => callWebApi({
	endpoint: `/api/words`,
	type: 'POST'
});

export const updateWord = async (word: IWord) => callWebApi({
	endpoint: `/api/words`,
	type: 'PUT'
});