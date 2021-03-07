import { translationStore } from '../stores';
import { ViewModel } from './core';

export class TranslationViewModel extends ViewModel {
    public constructor() {
        super(translationStore);
    }

    public getMessage(id: string | undefined): string | undefined {
        return id ? translationStore.getMessage(id) : id;
    }
}