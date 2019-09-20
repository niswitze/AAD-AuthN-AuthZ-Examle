namespace Microsoft.AspNetCore.Authentication
{
    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }

        //Not added by configuration wizard. Needed to obtain RedirectURI value from appSettings
        public string RedirectURI { get; set; }

        //Not added by configuration wizard. Needed to obtain AppIDURL value from appSettings
        public string AppIDURL { get; set; }

    }
}
