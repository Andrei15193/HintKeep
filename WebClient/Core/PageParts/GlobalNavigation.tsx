import React from "react";
import { Link } from "react-router";
import { InfoIcon } from "../Icons";

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
            <div title="Date and times are relative to the browser (device) timezone, everything else is displayed using English (GB) formatting.">
                <InfoIcon action />
            </div>
        </nav>
    );
}