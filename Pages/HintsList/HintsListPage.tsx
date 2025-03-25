import React from "react";
import { useUser } from "../Contexts/UserContext";

export function HintsListPage(): React.JSX.Element {
    const { username } = useUser()!;

    return (
        <>
            <h1>
                Hello
                {" "}
                {username}
                !
            </h1>
            <p>
                Sadly, you do not like storing hints... Maybe we can change that!
            </p>
        </>
    );
}