import { UserStore } from './user-store';
import { AlertsStore } from './alerts-store';
import { TranslationStore } from './translation-store';
import { enGB } from '../translations';

export const userStore = new UserStore();
export const alertsStore = new AlertsStore();
export const translationStore = new TranslationStore({ 'en-GB': enGB }, 'en-GB');