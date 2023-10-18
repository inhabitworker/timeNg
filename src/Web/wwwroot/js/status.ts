
// Online Status
    // register on connection changes func to try fetch
    // confirming reachable? then set state in original 
    // registrar dotnetobj 

function registerStatus(dotNetObject) {
    window.addEventListener("online", statusCheck)
    window.addEventListener("offline", statusCheck)

    async function statusCheck() {
        var isOnline = await fetch(hostPath(), { method: 'HEAD', mode: 'no-cors' })
            .then((response) => response && (response.ok || response.type === 'opaque'))
            .catch((error) => false && console.warn('[Connection test failed]:', error));

        // [JSInvokable]
        dotNetObject.invokeMethodAsync("SetOnline", isOnline);
    }
}

