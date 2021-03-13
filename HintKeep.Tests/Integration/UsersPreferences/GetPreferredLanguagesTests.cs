using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace HintKeep.Tests.Integration.UsersPreferences
{
    public class GetPreferredLanguagesTests : IClassFixture<HintKeepWebApplicationFactory>
    {
        private readonly HintKeepWebApplicationFactory _webApplicationFactory;

        public GetPreferredLanguagesTests(HintKeepWebApplicationFactory webApplicationFactory)
            => _webApplicationFactory = webApplicationFactory;

        [Fact]
        public async Task Get_WithoutAcceptLanguageHeader_ReturnsEmptyLanguagePreferences()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.Remove(HeaderNames.AcceptLanguage);

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }

        [Fact]
        public async Task Get_WithEmptyAcceptLanguageHeader_ReturnsEmptyLanguagePreferences()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.AcceptLanguage.Clear();

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }

        [Fact]
        public async Task Get_WithAnyAcceptLanguageHeader_ReturnsEmptyLanguagePreferences()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("*"));

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }

        [Fact]
        public async Task Get_WithOneAcceptLanguageHeader_ReturnsPreferredLanguage()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.Remove(HeaderNames.AcceptLanguage);
            client.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, "en;q=0.8,en-US;q=0.8,en-GB;q=1");

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "en-GB", "en", "en-US" }, await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }

        [Fact]
        public async Task Get_WithMultipleAcceptLanguageHeaders_ReturnsPreferredLanguage()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.Remove(HeaderNames.AcceptLanguage);
            client.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, new[] { "en;q=0.8" });
            client.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, new[] { "en-US;q=0.8" });
            client.DefaultRequestHeaders.Add(HeaderNames.AcceptLanguage, new[] { "en-GB;q=1" });

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "en-GB", "en", "en-US" }, await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }

        [Fact]
        public async Task Get_WithMultipleOptionsInOneAcceptLanguageHeader_ReturnsPreferredLanguage()
        {
            var client = _webApplicationFactory.CreateClient();
            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.8));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US", 0.8));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-GB", 1));

            var response = await client.GetAsync("/api/users/preferences/preferred-languages");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "en-GB", "en", "en-US" }, await response.Content.ReadFromJsonAsync<IEnumerable<string>>());
        }
    }
}