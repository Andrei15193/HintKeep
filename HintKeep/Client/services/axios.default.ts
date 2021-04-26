import axios from 'axios';

export const Axios = axios.create({
    validateStatus() {
        return true;
    }
});
Axios.interceptors.response.use(
    response => {
        const loginUrl = response.headers['x-login'];
        if (response.status === 401 && loginUrl)
            window.location = loginUrl;
        return response;
    }
);