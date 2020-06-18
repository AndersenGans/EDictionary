import { IWord } from "../../models/word";
import React from "react";
import { addWordRoutine, updateWordRoutine } from "../../routines";
import { connect } from "react-redux";

interface IWordProps { 
    word: IWord,
    addWord: (word: IWord) => void,
    updateWord: (word: IWord) => void
}

const Word: React.FunctionComponent<IWordProps> = ({ addWord, updateWord, word }) => {
    return (
        <>
            <div>{word.id}</div>
            <div>{word.word}</div>
            <div>{word.translation}</div>
            <div>{word.transcription}</div>
        </>
    )
}

const mapDispatchToProps = {
    addWord: addWordRoutine,
    updateWord: updateWordRoutine
}

export default connect(null, mapDispatchToProps)(Word);