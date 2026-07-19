import "./App.scss";

Promise
    .all([
        import("react"),
        import("react-dom/client"),
        import("./Pages/App"),
        import("react-model-view-viewmodel")
    ])
    .then(([{ createElement }, { createRoot }, { App: Startup }]) => {
        const appElement = document.getElementById("app")!;
        Array
            .from(appElement.children)
            .forEach((childNode) => childNode.remove());
        appElement.removeAttribute("class");

        createRoot(appElement)
            .render(createElement(Startup));
    });