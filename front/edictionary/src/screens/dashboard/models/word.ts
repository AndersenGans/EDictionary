import { IScoreModel } from "./scoreModel";

export interface IWord {
    id: number,
    word: string,
    translation: string,
    transcription: string,
    lastTouched: Date,
    allRepetitions: number,
    allMistakes: number,
    wasUsedInRandomPrinting: boolean,
    maxScoreEnglish: number,
    russianWordScoreModels: IScoreModel[]
}