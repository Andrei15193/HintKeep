import type { PropsWithChildren } from 'react';
import type { AxiosInstance } from 'axios';
import React, { createContext } from 'react';
import { Axios } from '../services/axios';
import { AlertsViewModel } from '../view-models/alerts-view-model';

export interface IServices {
    readonly axios: AxiosInstance;
    readonly alertsViewModel: AlertsViewModel;
};

export const Services: IServices = {
    axios: Axios,
    alertsViewModel: new AlertsViewModel()
};

export const ServicesContext = createContext<IServices>(Services);

export function ServicesProvider({ children }: PropsWithChildren<{}>): JSX.Element {
    return (
        <ServicesContext.Provider value={Services}>
            {children}
        </ServicesContext.Provider>
    );
}