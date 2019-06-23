---
layout: single
title:  "When Errors are Promises"
date:   2017-10-13 00:20:00 -0500
category: tech
---
Under what circumstances will the following code end up logging the rejected promise message to the console?
```javascript
foo()
    .catch((err) => {
        console.log(`Promise Rejected. reason = '${err}'`);
    });
```
When `foo` returns a rejected promise like so

```javascript
const foo = () => {
    return Promise.reject('Rejected Promise');
};
```

The console will look like
```
Promise Rejected. reason = 'Rejected Promise'
```
Now the naming of catch makes it seem like it should also handle errors. If `foo` looks like

```javascript
const foo = () => {
    throw new Error('Thrown Error');
};
```

The error won't be handled

```
/home/paul/errors_promises.js:3
 throw new Error('Thrown Error');
 ^

Error: Thrown Error
 at throwSimpleError (/home/paul/errors_promises.js:3:11)
```

This happens because the error wasn't being thrown from within a resolving promise and as such won't be treated as a rejection. If foo looks like

```javascript
const throwErrorFromPromise = () => {
    return Promise.resolve('Error from Promise')
        .then(() => {
            throw new Error('Thrown Error');
        });
};

```
then the thrown error will now catch!

```
Promise Rejected. reason = 'Error: Thrown Error'
```

Since the error is thrown inside the resolution of the promise it will be treated as a rejected promise. However it is worth noting that asynchronous thrown errors won't be treated as rejected promises even if they are within a resolving promise.

```javascript
const foo = () => {
    return Promise.resolve('Async Thrown Error from Promise')
        .then(() => {
            setTimeout(() => {
                throw new Error('Async Error');
            }, 0);
        });
};
```

The console would look like
```
/home/paul/errors_promises.js:21
 throw new Error('Async Error');
 ^

Error: Async Error
 at Timeout.setTimeout [as _onTimeout] (/home/paul/errors_promises.js:21:23)
 ```
