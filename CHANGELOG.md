CHANGELOG:
# 1.4.0

Dependency changed: LitJson (new).

New features:
- Cache remote config data as JSON to PlayerPref.
- Add default config as scriptable object.
- Make new extra package that include Firebase manager prefab, Default config files, Firebase cloud messaging helper.
- Accessing Firebase manager instance will automatically instantiate an instance using Resouces.Load.

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
