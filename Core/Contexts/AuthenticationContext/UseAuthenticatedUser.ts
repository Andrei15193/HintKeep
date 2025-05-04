import type { IUser } from "../../Models";
import { useAuthentication } from "./AuthenticationContext";

/**
 * Gets the currently authenticated user.
 * @returns ({@link IUser}) The authenticated user.
 * @throws When there is no authenticated user.
 */
export function useAuthenticatedUser(): IUser {
    const user = useAuthentication().user;

    if (user === null)
        throw Error("Expected user to be authenticated.");

    return user;
}