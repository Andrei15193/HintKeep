import axios from 'axios';

export const Axios = axios.create({
    baseURL: 'https://hintkeep.azurewebsites.net/',
    validateStatus() {
        return true;
    }
});