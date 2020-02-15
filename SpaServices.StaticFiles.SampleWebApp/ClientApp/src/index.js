import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');
ReactDOM.render(<App baseUrl={baseUrl} />, rootElement);
