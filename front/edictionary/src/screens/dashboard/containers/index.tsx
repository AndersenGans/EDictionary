import React from "react";
import { connect } from "react-redux";
import WordList from "../components/wordsList";
import { fetchWordsRoutine } from "../routines";

interface IWordsContainer {
	fetchWords: () => void,
}

const WordsContainer: React.FunctionComponent<IWordsContainer> = ({ fetchWords }) => {
	fetchWords();

	return (
        <WordList />
    )
}

const mapDispatchToProps = {
    fetchWords: fetchWordsRoutine
}


export default connect(null, mapDispatchToProps)(WordsContainer);