window.highlightCodeSnippet = (code, options) => {
    let highlightResult = hljs.highlight(code, options);
    return highlightResult.value;
}
