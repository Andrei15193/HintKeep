import axios from 'axios';

export const Axios = axios.create({
    validateStatus() {
        return true;
    }
});

Axios.interceptors.request.use(
    request => {
        if (request.headers)
            if (request.headers.Authorization)
                localStorage.setItem('authorization', request.headers.Authorization);
            else if (localStorage.getItem('authorization'))
                Object.assign(request.headers, { Authorization: localStorage.getItem('authorization') });
        return request;
    }
);

Axios.interceptors.response.use(
    response => {
        if (response.status === 401)
            localStorage.removeItem('authorization');
        return response;
    }
);