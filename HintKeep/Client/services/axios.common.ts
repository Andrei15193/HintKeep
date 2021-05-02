import type { AxiosInstance } from "axios";

export function configureDefaultListeners(axios: AxiosInstance): AxiosInstance {
    axios.interceptors.response.use(
        response => {
            const loginUrl = response.headers['x-login'];
            if (response.status === 401 && loginUrl)
                window.location = loginUrl;
            return response;
        }
    );

    return axios;
}