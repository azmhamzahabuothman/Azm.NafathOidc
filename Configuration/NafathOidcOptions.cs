namespace Azm.NafathOidc.Configuration
{
    public class NafathOidcOptions
    {
        public const string SectionName = "NafathOidc";

        public string BaseUrl { get; set; } = default!;
        public string AppId { get; set; } = default!;
        public string AppKey { get; set; } = default!;
    }
}
