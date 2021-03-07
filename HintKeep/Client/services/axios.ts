import axios from 'axios';

export const Axios = axios.create({
    validateStatus() {
        return true;
    }
});