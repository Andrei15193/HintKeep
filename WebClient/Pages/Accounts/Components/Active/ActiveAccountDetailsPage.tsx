import type { NonIndexRouteObject } from "react-router";
import React from "react";
import { ViewEditToggle } from "../../../../Core/ViewEditToggle";
import { ActiveAccountDetailsPageEdit } from "./ActiveAccountDetailsPageEdit";
import { ActiveAccountDetailsPageView } from "./ActiveAccountDetailsPageView";

export const ActiveAccountDetailsRoute: NonIndexRouteObject = {
    path: "/:id",
    Component: ActiveAccountDetailsPage
};

function ActiveAccountDetailsPage(): React.JSX.Element {
    return (
        <ViewEditToggle
            viewComponent={ActiveAccountDetailsPageView}
            editComponent={ActiveAccountDetailsPageEdit}
        />
    );
}