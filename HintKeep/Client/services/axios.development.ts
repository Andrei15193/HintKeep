import { Axios } from './axios.default';
export { Axios };

Axios.interceptors.request.use(
    request => {
        if (localStorage.getItem('authorization'))
            request.headers.Authorization = localStorage.getItem('authorization');
        else if (request.headers.Authorization)
            localStorage.setItem('authorization', request.headers.Authorization);
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