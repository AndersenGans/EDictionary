import React from 'react';
import './App.scss';
import { Provider } from 'react-redux';
import { store } from '../../store';
import WordsContainer from '../../screens/dashboard/containers/index';

const App: React.FC = () => (
  <Provider store={store}> 
    <WordsContainer />  
  </Provider>
);

export default App;
