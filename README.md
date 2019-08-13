# AuthN/AuthZ with Azure Active Directory

### Version 1.0 - .NET Core with ADAL

#### Steps to run sample:

1. Clone or Fork/Clone this repository
2. Rename both appSettingsReference.json to appSettings.json
3. Create two app registrations to for the ModernAuth UI and ModernAuth API
    1. Give the ModernAuth API the following Exposed API scope:
        ![ExposeModernAuthAPI](/images/ExposeModernAuthAPI.PNG)
    2. Give the ModernAuth UI the following permissions:
        ![ModernAuthUIPermissions](/images/ModernAuthUIPermissions.PNG)
    3. Give the ModernAuth API the following permissions:
        ![ModernAuthAPIPermissions](/images/ModernAuthAPIPermissions.PNG)
    4. Copy the Application ID of the ModernAuth UI App registration and place it in the knownClientApplications element        within the ModernAuth API's app manifest
        ![KnownClientApplicationsAppManifest](/images/KnownClientApplicationsAppManifest.PNG)
4. Update the appSettings.json in both the ModernAuth UI and ModernAuth API to reflect all need settings from their app registrations.
