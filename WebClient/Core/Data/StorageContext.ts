import { DependencyToken, ViewModel } from "react-model-view-viewmodel";

export type StorageType = "IndexedDB" | "HintKeepAPI";

export interface IStorageContext {
    readonly storageType: StorageType;

    useIndexedDB(): void;

    useHintKeepApi(): void;
}

export const StorageContext = new DependencyToken<IStorageContext>("storage context");

export class StorageContextService extends ViewModel implements IStorageContext {
    private _storageType: StorageType;

    public constructor(defaultStorageType: StorageType) {
        super();
        this._storageType = defaultStorageType;
    }

    public get storageType(): StorageType {
        return this._storageType;
    }

    public useIndexedDB(): void {
        if (this._storageType !== "IndexedDB") {
            this._storageType = "IndexedDB";
            this.notifyPropertiesChanged("storageType");
        }
    }

    public useHintKeepApi(): void {
        if (this._storageType !== "HintKeepAPI") {
            this._storageType = "HintKeepAPI";
            this.notifyPropertiesChanged("storageType");
        }
    }
}