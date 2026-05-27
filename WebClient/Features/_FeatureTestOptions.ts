export interface IFeatureTestOptions {
    readonly templateFilePath: string | null | undefined;
    readonly failedScenarioHtmlSnapshotDirectoryPath: string | null | undefined;
    readonly stepHtmlSnapshotDirectoryPath: string | null | undefined;
}

export const featureTestOptions: IFeatureTestOptions = {
    templateFilePath: process.env.templateFilePath,
    failedScenarioHtmlSnapshotDirectoryPath: process.env.failedScenarioHtmlSnapshotDirectoryPath,
    stepHtmlSnapshotDirectoryPath: process.env.stepHtmlSnapshotDirectoryPath
};