import type { AxiosInstance } from 'axios';
import { DependencyToken } from 'react-model-view-viewmodel';

export { Axios as AxiosInstance } from './axios';

export const Axios = new DependencyToken<AxiosInstance>('Axios');