import axios from 'axios';
import { configureDefaultListeners } from './axios.common';

export const Axios = configureDefaultListeners(
    axios.create({
        baseURL: 'https://hintkeep.azurewebsites.net/',
        validateStatus() {
            return true;
        }
    })
);