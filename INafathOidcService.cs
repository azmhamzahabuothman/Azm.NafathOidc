using Azm.NafathOidc.Models;

namespace Azm.NafathOidc
{
    public interface INafathOidcService
    {
        Task<NafathOidcSessionResponse?> InitiateSession(string locale, string requestId);
        Task<NafathOidcJwtResponse?> ExchangeStateForJwt(string state);
        Task<NafathOidcValidateTokenResponse?> ValidateIdToken(string idToken);
        Task<NafathOidcJwkSet?> GetJwk();
    }
}
