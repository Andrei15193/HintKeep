export interface IDataSource<TOptions, TResult> {
    getDataAsync(options: TOptions): Promise<TResult>;
}