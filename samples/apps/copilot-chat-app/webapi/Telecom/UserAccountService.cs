using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SemanticKernel.Service.Telecom
{
    public class UserAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _searchEndpoint;

        public UserAccountService(HttpClient httpClient, IConfiguration configuration)
        {
            this._httpClient = httpClient;
            this._baseUrl = configuration.GetValue<string>("TelecomAuth:BaseUrl");
            this._searchEndpoint = configuration.GetValue<string>("TelecomAuth:SearchEndpoint");
        }

        public async Task<UserAccount> GetUserAccountFromTokenAsync(string token)
        {
            var url = $"{this._baseUrl}{this._searchEndpoint}?token={token}";
            var response = await this._httpClient.GetAsync(url).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var userAccountResponse = JsonConvert.DeserializeObject<UserAccountResponse>(json);

            return userAccountResponse.rows.FirstOrDefault();
        }

    }
}
