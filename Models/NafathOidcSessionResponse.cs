namespace Azm.NafathOidc.Models
{
    public class NafathOidcSessionResponse
    {
        public string Id { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string HashedState { get; set; } = default!;
        public string RequestId { get; set; } = default!;
    }
}
