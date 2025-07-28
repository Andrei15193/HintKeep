export function mapDbRequestToPromise<TResult>(dbRequest: IDBRequest<any>): Promise<TResult> {
    return new Promise<TResult>((resolve, reject) => {
        dbRequest.addEventListener("success", () => {
            resolve(dbRequest.result);
        });
        dbRequest.addEventListener("error", () => {
            reject(dbRequest.error);
        });
    });
}