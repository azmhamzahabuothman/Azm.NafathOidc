using Azm.NafathOidc.Configuration;
using Azm.NafathOidc.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Serilog;

namespace Azm.NafathOidc
{
    public class NafathOidcService : INafathOidcService, IDisposable
    {
        private readonly RestClient _client;
        private readonly NafathOidcOptions _options;

        public NafathOidcService(IOptions<NafathOidcOptions> options)
        {
            _options = options.Value;
            _client = new RestClient(new RestClientOptions(_options.BaseUrl));
        }

        public async Task<NafathOidcSessionResponse?> InitiateSession(string locale, string requestId)
        {
            var request = new RestRequest("api/v2/oidc/session", Method.Get);
            AddAuthHeaders(request);
            request.AddQueryParameter("locale", locale);
            request.AddQueryParameter("requestId", requestId);

            var response = await Execute<NafathOidcSessionResponse>(request);
            return response.IsSuccessful ? response.Data : null;
        }

        public async Task<NafathOidcJwtResponse?> ExchangeStateForJwt(string state)
        {
            var request = new RestRequest("api/v2/oidc/jwt", Method.Post) { RequestFormat = DataFormat.Json };
            AddAuthHeaders(request);
            request.AddBody(new { state });

            var response = await Execute<NafathOidcJwtResponse>(request);
            return response.IsSuccessful ? response.Data : null;
        }

        public async Task<NafathOidcValidateTokenResponse?> ValidateIdToken(string idToken)
        {
            var request = new RestRequest("api/v2/oidc/jwt/valid", Method.Post) { RequestFormat = DataFormat.Json };
            AddAuthHeaders(request);
            request.AddBody(new { id_token = idToken });

            var response = await Execute<NafathOidcValidateTokenResponse>(request);
            return response.IsSuccessful ? response.Data : null;
        }

        public async Task<NafathOidcJwkSet?> GetJwk()
        {
            var request = new RestRequest("api/v2/oidc/jwk", Method.Get);
            AddAuthHeaders(request);

            var response = await Execute<NafathOidcJwkSet>(request);
            return response.IsSuccessful ? response.Data : null;
        }

        private void AddAuthHeaders(RestRequest request)
        {
            request.AddOrUpdateHeader("APP-ID", _options.AppId);
            request.AddOrUpdateHeader("APP-KEY", _options.AppKey);
        }

        private async Task<RestResponse<T>> Execute<T>(RestRequest request)
        {
            var response = await _client.ExecuteAsync<T>(request);

            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            string serializedBody = body != null ? $" with body {JsonConvert.SerializeObject(body)}" : "";
            Log.Information("NafathOidc /{Method}:{Resource}{Body} => Code:{StatusCode};Content:{Content}",
                request.Method, request.Resource, serializedBody, response.StatusCode, response.Content);

            return response;
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
