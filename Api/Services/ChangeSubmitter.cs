using Api.ApiModels;
using StateManagment.Models;

namespace Api.Services
{
    public class ChangeSubmitter(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        public async Task<List<ChangeSubmitResult>> SubmitAll(ChangeLink[] changeLinks)
        {
            var results = new List<ChangeSubmitResult>();
            var httpClient = httpClientFactory.CreateClient();

            var request = httpContextAccessor.HttpContext?.Request;
            var host = request is not null
                ? $"{request.Scheme}://{request.Host}"
                : throw new InvalidOperationException("Unable to resolve the current host name.");

            foreach (var link in changeLinks)
            {
                var result = new ChangeSubmitResult
                {
                    Name = link.Name,
                    EntityId = link.EntityId,
                    State = link.State,
                    DraftVersion = link.DraftVersion,
                    SubmittedVersion = link.SubmittedVersion,
                    Link = link.Link
                };

                if (string.IsNullOrEmpty(link.Link))
                {
                    result.Result = "Failed";
                    results.Add(result);
                    continue;
                }

                try
                {
                    var url = $"{host}{link.Link}";
                    var body = new SubmitEntityModel { TargetVersion = link.DraftVersion };
                    var response = await httpClient.PostAsJsonAsync(url, body);

                    result.Result = response.IsSuccessStatusCode ? "Submitted" : "Failed";
                }
                catch
                {
                    result.Result = "Failed";
                }

                results.Add(result);
            }

            return results;
        }
    }

    public class ChangeSubmitResult
    {
        public EntityName Name { get; set; }
        public string EntityId { get; set; }
        public EntityState State { get; set; }
        public int DraftVersion { get; set; }
        public int SubmittedVersion { get; set; }
        public string? Link { get; set; }
        public string Result { get; set; }
    }
}
