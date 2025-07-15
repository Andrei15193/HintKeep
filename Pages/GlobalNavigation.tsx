import React from "react";
import { Link } from "react-router";

export function GlobalNavigation(): React.JSX.Element {
    return (
        <nav>
            <Link to="/">
                Accounts
            </Link>
            <Link to="/archived">
                Archived Accounts
            </Link>
            <Link to="/profile">
                Profile
            </Link>
        </nav>
    );
}