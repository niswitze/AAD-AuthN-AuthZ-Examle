# AuthN/AuthZ with Azure Active Directory

### Version 1.0 - .NET Core with ADAL

#### Steps to run sample:

1. Clone or Fork/Clone this repository
2. Rename both appSettingsReference.json to appSettings.json
3. Create two app registrations to for the ModernAuth UI and ModernAuth API
    1. Give the ModernAuth API the following Exposed API scope:
        [EOF]
        ![ExposeModernAuthAPI](/images/ExposeModernAuthAPI.PNG)
        [EOF]
    2. Give the ModernAuth UI the following permissions:
        [EOF]
        ![ModernAuthUIPermissions](/images/ModernAuthUIPermissions.PNG)
    3. Give the ModernAuth API the following permissions:
        [EOF]
        ![ModernAuthAPIPermissions](/images/ModernAuthAPIPermissions.PNG)
        [EOF]
    4. Copy the Application ID of the ModernAuth UI App registration and place it in the knownClientApplications element        within the ModernAuth API's app manifest
        [EOF]
        ![KnownClientApplicationsAppManifest](/images/KnownClientApplicationsAppManifest.PNG)
        [EOF]
4. Update the appSettings.json in both the ModernAuth UI and ModernAuth API to reflect all need settings from their app registrations.
