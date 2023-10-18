const hostPath = () => location.protocol + "//" + location.host; 

/// Config:
    // fetch config and push variables into css -> more elegant than in app code, allows styling loading screen etc.

// fetch and set css from api
const getCss = async (): Promise<void> => {
    var endpoint = "/api/config/css";
    var css: CssProperties = await fetch(hostPath() + endpoint)
        .then((response) => response.json());

    setCss(css);
}

// fetch and set (dictionary of properties/value)
const setCss = (cssProps: CssProperties): void => {
    // var root = document.getRootNode();
    let root = document.querySelector(":root");
    if (!(root instanceof HTMLElement)) return;

    // let rootStyle = () => getComputedStyle(root);

    // for init
    root.style.setProperty("visibility", "visible");

    for (let key in cssProps) {
        let value = cssProps[key];
        root.style.setProperty(key, value);
    }
}

/// UI
    // stuff for ui

// disable default behaviour within special text box used inside form for tag chooser
const disableSubmit = (): void => {
    let inputters = window.document.getElementsByClassName("inputTagText");

    Array.prototype.forEach.call(inputters,
        (el) => el.onkeypress = (e) => {
            if (e.keyCode == 13) {
                e.preventDefault();
            }
        }
    );
}

/// Active interval ticking:
    // I was previously doing this as a blazor server app and this seemed to make more sense
    // maybe less so now but even still probably nicer than disposing timer etc

// format single digit hrs to leading 0
function leading(input: number): string {
    if (input / 10 < 1) {
        return `0${input}`;
    }
    return input.toString();
};

// keep active times ticking
const elapsed = (start: string): void => {

    let elapsedElements = window.document.getElementsByClassName("elapsedActive");

    let startDate = new Date(start);
    let time = new Date();

    let duration = new Date(time.getTime() - startDate.getTime());
    let hours = Math.floor( duration.getTime() / (1000 * 60 * 60) );
    let newString = leading(hours) + ":" + leading(duration.getUTCMinutes()) + ":" + leading(duration.getUTCSeconds());

    console.log(elapsedElements);
    Array.prototype.forEach.call(elapsedElements, (e) => e.innerHTML = newString);

    setTimeout(elapsed, 1000, start);
};

const downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
