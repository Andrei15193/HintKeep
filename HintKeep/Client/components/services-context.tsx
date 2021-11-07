import type { PropsWithChildren } from 'react';
import type { AxiosInstance } from 'axios';
import React, { createContext } from 'react';
import { Axios } from '../services/axios';
import { AlertsViewModel } from '../view-models/alerts-view-model';
import { SessionViewModel } from '../view-models/session-view-model';

export interface IServices {
    readonly axios: AxiosInstance;
    readonly alertsViewModel: AlertsViewModel;
    readonly sessionViewModel: SessionViewModel;
};

export const Services: IServices = {
    axios: Axios,
    alertsViewModel: new AlertsViewModel(),
    sessionViewModel: new SessionViewModel(Axios)
};

export const ServicesContext = createContext<IServices>(Services);

export function ServicesProvider({ children }: PropsWithChildren<{}>): JSX.Element {
    return (
        <ServicesContext.Provider value={Services}>
            {children}
        </ServicesContext.Provider>
    );
}