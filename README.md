# AuthN/AuthZ with Azure Active Directory

### Version 1.0 - .NET Core with ADAL

#### Steps to run sample:

1. Clone or Fork/Clone this repository
2. Rename both appSettingsReference.json to appSettings.json
3. Create two app registrations to for the ModernAuth UI and ModernAuth API
    1. Give the ModernAuth API the following Exposed API scope:
        <img src="/images/ModernAuthUIPermissions.PNG" alt="ModernAuthUIPermissions Image" style="border: 1px solid black" />

    2. Give the ModernAuth UI the following permissions:
        ![ModernAuthUIPermissions](/images/ModernAuthUIPermissions.PNG# border)

    3. Give the ModernAuth API the following permissions:
        ![ModernAuthAPIPermissions](/images/ModernAuthAPIPermissions.PNG# border)

    4. Copy the Application ID of the ModernAuth UI App registration and place it in the knownClientApplications element        within the ModernAuth API's app manifest
        ![KnownClientApplicationsAppManifest](/images/KnownClientApplicationsAppManifest.PNG# border)

4. Update the appSettings.json in both the ModernAuth UI and ModernAuth API to reflect all need settings from their app registrations.


img[src~="border"] {
   border: 1px solid black;
}
