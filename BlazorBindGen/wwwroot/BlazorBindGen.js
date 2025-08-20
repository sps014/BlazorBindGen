const eventHandlerMap = new Map();

export function addEventListenerCSharp(obj, eventName, funcName, dotnet) {
    const key = "addEventListener";
    if (!(key in obj)) {
        console.error(`cant attach event ${eventName} due to unavailability of ${key}() `);
        return;
    }

    // unique key per subscription
    const subscriptionKey = `${eventName}_${funcName}`;

    // create handler once
    const handler = (...args) => dotnet.invokeMethod(funcName, ...args);

    // store it
    eventHandlerMap.set(subscriptionKey, handler);

    obj[key](eventName, handler);
}

export function removeEventListenerCSharp(obj, eventName, funcName) {
    const key = "removeEventListener";
    if (!(key in obj)) {
        console.error(`cant detach event ${eventName} due to unavailability of ${key}() `);
        return;
    }

    const subscriptionKey = `${eventName}_${funcName}`;
    const handler = eventHandlerMap.get(subscriptionKey);

    if (handler) {
        obj[key](eventName, handler);
        eventHandlerMap.delete(subscriptionKey);
    }
}
