interface Interval {
    start: Date,
    end?: Date,
    tags: string[],
    annotation?: string,
    updated: Date
}

interface CssProperties {
    [index: string]: string;
}

interface Config {
    Theme: ITheme;
    Highlight: string;
    Colours: ColourMatch[];
}

interface ColourMatch {
    Colour: string;
    Regex: string;
}

interface ITheme {
    Foreground: string,
    Shade: string,
    Background: string 
}

declare class Light implements ITheme {
    Foreground: black;
    Shade: dimGray;
    Background:  white;
}

declare class Dark implements ITheme {
    Foreground: white;
    Shade: dimGray;
    Background: darkBg;
}

declare class Black implements ITheme {
    Foreground: white;
    Shade: gray;
    Background: black;
}

type white = "#FFFFFF";
type dimGray = "#696969";
type gray = "#808080";
type black = "#000000";
type darkBg = "#1E1E1E";

