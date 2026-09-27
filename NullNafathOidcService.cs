using Azm.NafathOidc.Models;

namespace Azm.NafathOidc
{
    public class NullNafathOidcService : INafathOidcService
    {
        public Task<NafathOidcJwkSet?> GetJwk()
        {
            return Task.FromResult<NafathOidcJwkSet?>(new NafathOidcJwkSet
            {
                Keys = new List<NafathOidcJwk>
                {
                    new() { Kty = "RSA", Kid = "elm", Use = "sig", Alg = "RS256" }
                }
            });
        }

        public Task<NafathOidcSessionResponse?> InitiateSession(string locale, string requestId)
        {
            return Task.FromResult<NafathOidcSessionResponse?>(new NafathOidcSessionResponse
            {
                Url = $"https://nafath-mock/authorize?requestId={requestId}&locale={locale}",
                HashedState = Guid.NewGuid().ToString("N"),
                RequestId = requestId,
                Id = null!
            });
        }

        public Task<NafathOidcJwtResponse?> ExchangeStateForJwt(string state)
        {
            return Task.FromResult<NafathOidcJwtResponse?>(new NafathOidcJwtResponse
            {
                Token = "mock-id-token"
            });
        }

        public Task<NafathOidcValidateTokenResponse?> ValidateIdToken(string idToken)
        {
            return Task.FromResult<NafathOidcValidateTokenResponse?>(new NafathOidcValidateTokenResponse
            {
                Valid = true
            });
        }
    }
}
