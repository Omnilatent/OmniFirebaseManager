## Dependencies

- Firebase Analytic 7.2.0
- Firebase Remote Config 7.2.0
- Firebase Messaging 7.2.0 (if you use Cloud Message)

## SETUP:

Install required Firebase component through Package Manager.

Add a gameobject to Main scene with the following components:

- FirebaseManager
- FirebaseRemoteConfigHelper (if you use Remote Config)
- FirebaseCloudMessagingHelper (if you use Cloud Message)

## USAGE:
    
Call FirebaseManager.LogEvent to log events.
- Log an event:
`FirebaseManager.LogEvent(string name)`

- Log an event with parameter:
    ```
    FirebaseManager.LogEvent(string name, string paramName, string value)
    FirebaseManager.LogEvent(string name, string paramName, int value)
    FirebaseManager.LogEvent(string name, string paramName, float value)
    ```
    
- Log an array of parameters:
`FirebaseManager.LogEvent(string name, Firebase.Analytics.Parameter[] array)`
    
To construct an array: 
```
Firebase.Analytics.Parameter[] array = new Firebase.Analytics.Parameter[]{
    new Firebase.Analytics.Parameter(string eventName, string eventParam)
    }
```
    
- In case you need to log event early in the game (example: In the opening scene) and the log event code can be executed before Firebase initialization is completed, you need to make sure Firebase initialization is completed first before you log event.

Use these if you want to wait for Firebase to be ready before logging events
```
    if (FirebaseManager.FirebaseReady)
        OnFirebaseReady(this, true);
    else FirebaseManager.handleOnReady += OnFirebaseReady;
```

`FirebaseManager.CheckWaitForReady(System.EventHandler<bool> callback)`  
callback: when Firebase initialization completes, this function will be called.

`FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(System.EventHandler<bool> callback)`  
callback: when Firebase Remote Config initialization completes, this function will be called.

- For example, you can write a loading code like this in the opening scene to make sure Firebase is ready before any code in the game is executed:
```
IENumerator Start() {
    bool? isFirebaseReady = false;
    FirebaseManager.CheckWaitForReady((sender, success) => {
        isFirebaseReady = success;
    });
    
    //Wait until Firebase is ready
    while (!isFirebaseReady.HasValue) {
        yield return new WaitForSeconds(0.1f);
    }
    
    //Firebase has completed initialization
    if (isFirebaseReady)
        FirebaseManager.LogEvent("Firebase_is_ready");
}
```
