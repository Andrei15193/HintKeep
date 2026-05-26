export interface IFeatureTestOptions {
    readonly htmlSnapshotDirectoryPath: string | null | undefined;
    readonly templateFilePath: string | null | undefined;
}

export const featureTestOptions: IFeatureTestOptions = {
    htmlSnapshotDirectoryPath: process.env.htmlSnapshotDirectoryPath,
    templateFilePath: process.env.templateFilePath
};