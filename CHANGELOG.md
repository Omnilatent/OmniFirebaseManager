CHANGELOG:

# 1.5.1

### Firebase & Threading Improvements
* **Main Thread Dependency Check:** Updated `CheckDependencies` to execute properly on the main thread. 0cfa7a1
* **iOS Firebase Lifecycle Fix:** Fixed an iOS bug preventing Firebase functions from being invoked prior to `CheckDependencies` completion. Implemented via a queued callback to the main thread inside `ContinueWith()` (addressing Firebase Unity SDK issue #1075). 3aa7eee
* **Remote Config Optimization:** Modified `FirebaseRemoteConfigHelper` to return early if an instance already exists. 0d1d340
* **Logging Defaults:** Changed `enableLogToFirebase` configuration to be enabled (`true`) by default. 6c29ce6

### Data Bucket Features & Fixes
* **Race Condition Resolution:** Fixed an initialization race condition where `databucket` would awake before the `firebase manager`. 0764558
* **Real-Time Logs:** Added a toggle feature for `databucket` real-time logging. 7f1dfae
* **Setup UI:** Introduced an initial setup window featuring an installation button for `JACAT_DATABUCKET`. 56969f0
* **Feature Rollout:** Integrated core Data Bucket features. 474c4e0

### General Bug Fixes & Refactoring
* **UI/Label Fix:** Resolved an issue where extra packages were missing the "Package" label. ffa9b1c
* **Code Refactoring:** Explicitly defined types for null values to improve type safety. c0fa5a9

# 1.5.0
Dependency change: Firebase SDK 12.0.0
- Change `setting.MinimumFetchInternalInMilliseconds` to `setting.MinimumFetchIntervalInMilliseconds` to support Firebase 12.0.0.
- Add try catch to `int GetInt(string key, int defaultValue = 0)`.

# 1.4.2
Fix:
- Editor files: Rename namespace Firebase to FirebaseNS

# 1.4.1

News:
- Add toggle log console to serialize field of Firebase Manager component.
- Add check parameter value length if it exceeds 100 characters.

Fixes:
- Use static is debug build so Log event doesn't require to be called from main thread.
- Only assign status of initSuccess if initSuccess has not been set to fix issue with initSuccess being set incorrectly when an exception happens during callback.

Changes:
- Change Firebase remote config helper's instance to property. An instance will be created on demand to allow testing from any scene.
- Add params modifier to LogEvent's array parameter to allow convenient usage.

# 1.4.0

Dependency changed: LitJson (new).

New features:
- Cache remote config data as JSON to PlayerPref.
- Add default config as scriptable object.
- Make new extra package that include Firebase manager prefab, Default config files, Firebase cloud messaging helper.
- Accessing Firebase manager instance will automatically instantiate an instance using Resouces.Load.

# 1.3.1
Changes:
- Clean up code. 
- Log installation id to console.
- Remove unnecessary conditional scripting define symbol in FirebaseRemoteConfigHelper.

# 1.3.0
News:
- Scripting define symbol "OMNILATENT_FIREBASE_MANAGER" will automatically be added to project.

Change:
- Moved Firebase Remote Config Helper class from Omni Ads Manager to Omni Firebase Manager package.

# 1.2.2
Change:
- When manually logging exception, add option to log it to console as well

Fix:
- Remove duplicate log exception to console.

# 1.2.1
Fix:
- Add log ScreenClass to Log screen view.
- Change CheckEventNameValid: change error to exception and log exception to Firebase.

# 1.2.0
Changes:
- Update Log screen view code to be compatible with Firebase 8.6.2.
Now require Firebase 8.6.2.

# 1.1.2
New features:
- Log most event to console to include check param and value logged.
- Check valid event name & parameters.

Changes:
- Update asmdef.
- Organise scripts to one folder.
- Include cloud messaging helper as extra script package.

# 1.1.1: 
24/6/2020: Add LogException & Crashlytic Log

# 1.1.0: 
5/5: change CheckDependenciesAsync function to async to work with remote config manager
