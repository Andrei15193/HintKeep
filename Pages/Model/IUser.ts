import { DependencyToken } from "react-model-view-viewmodel";

export const User = new DependencyToken<IUser>("user");

export interface IUser {
    readonly username: string;
}