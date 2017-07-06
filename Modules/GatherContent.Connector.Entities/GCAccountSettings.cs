namespace GatherContent.Connector.Entities
{
    public class GCAccountSettings
    {
        public string ApiUrl { get; set; }

        public string Username { get; set; }

        public string ApiKey { get; set; }

        public string TemplateFolderId { get; set; }

        public string DateFormat { get; set; }
        public string ImportDateFormat { get; set; }
        public string AccountItemId { get; set; }

        public string GatherContentUrl { get; set; }

        public string DropTreeHomeNode { get; set; }
    }
}
