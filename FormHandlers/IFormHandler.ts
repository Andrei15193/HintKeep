export interface IFormHandler<TForm> {
    handleAsync(form: TForm): Promise<void>;
}