import type { NonIndexRouteObject } from "react-router";
import React from "react";
import { ViewEditToggle } from "../../../../Core/ViewEditToggle";
import { AccountDetailsPageEdit } from "./AccountDetailsPageEdit";
import { AccountDetailsPageView } from "./AccountDetailsPageView";

export const ActiveAccountDetailsRoute: NonIndexRouteObject = {
    path: "/:id",
    Component: ActiveAccountDetailsPage
};

function ActiveAccountDetailsPage(): React.JSX.Element {
    return (
        <ViewEditToggle
            viewComponent={AccountDetailsPageView}
            editComponent={AccountDetailsPageEdit}
        />
    );
}