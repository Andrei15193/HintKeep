export interface IIndexedDatabaseDefinition {
    readonly name: string;
    readonly structureDefinitions: readonly IDatabaseStructureDefinition[];
}

export interface IDatabaseStructureDefinition {
    configure(database: IDBDatabase): void;
}