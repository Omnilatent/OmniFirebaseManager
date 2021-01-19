SETUP:
Install required Firebase component through Package Manager.
Add a gameobject to Main scene with the following components:
- FirebaseManager
- FirebaseRemoteConfigHelper (if you use Remote Config)
- FirebaseCloudMessagingHelper (if you use Cloud Message)

Add these codes if you want to wait for Firebase to be ready before logging events
    if (FirebaseManager.FirebaseReady) OnFirebaseReady(this, true);
    else FirebaseManager.handleOnReady += OnFirebaseReady;

Use FirebaseManager.CheckWaitForReady(System.EventHandler<bool> callback)
with callback as the function to call when Firebase has completed setup

Use FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig(System.EventHandler<bool> callback)
with callback as the function to call when Firebase Remote has completed setup

USAGE:
Call FirebaseManager.LogEvent to log events.
