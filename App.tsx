import "./App.scss";

Promise
    .all([
        import("react"),
        import("react-dom/client"),
        import("./Pages/Startup"),
        import("react-model-view-viewmodel")
    ])
    .then(([{ createElement }, { createRoot }, { Startup }]) => {
        const appElement = document.getElementById("app")!;
        appElement.removeAttribute("class");
        appElement.childNodes.forEach((childNode) => childNode.remove());

        createRoot(appElement)
            .render(createElement(Startup));
    });