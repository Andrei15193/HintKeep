import { useContext } from "react";
import { type IUserContext, UserContext } from "./UserContext";

export function useUserContext(): IUserContext {
    return useContext(UserContext)!;
}