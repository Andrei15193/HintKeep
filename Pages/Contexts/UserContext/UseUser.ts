import type { IUser } from "../../Model/IUser";
import { useContext } from "react";
import { UserContext } from "./UserContext";

export function useUser(): IUser | null {
    return useContext(UserContext)!.user;
}