export interface IIndexedDatabaseConnectionInfo {
    readonly name: string;
    readonly version: number;
    readonly databaseMigrations: readonly IDatabaseMigration[];
}
export interface IDatabaseMigration {
    readonly version: number;

    applyChanges(database: IDBDatabase): void;

    revertChanges(database: IDBDatabase): void;
}