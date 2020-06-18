import { IWord } from "../../models/word";
import Word from "../word";
import React from "react";
import { connect } from 'react-redux';

interface IWordListProps { 
    words: IWord[],
    repetitions: number
}

const WordList: React.FunctionComponent<IWordListProps> = ({  words, repetitions }) => {
    return (
        <>
            {
                words && words.map((word) => 
                    <Word word={word} />
                )
            }            
        </>
    )
}

const mapStateToProps = (state: any) => {
    const { words, repetitions } = state;

    return {
        words,
        repetitions
    };
};

export default connect(mapStateToProps)(WordList);