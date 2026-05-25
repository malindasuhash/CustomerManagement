using Api.ApiModels;
using StateManagment.Models;

namespace Api.Services
{
    public class ChangeSubmitter(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        public const string SubmitAction = "Submitted";
        public const string FailedAction = "Failed";
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
                    result.Result = FailedAction;
                    results.Add(result);
                    continue;
                }

                try
                {
                    var url = $"{host}{link.Link}";
                    var body = new ApiContract.SubmitActionRequest { Target_draft_version = (long)link.DraftVersion };
                    var response = await httpClient.PostAsJsonAsync(url, body);

                    result.Result = response.IsSuccessStatusCode ? SubmitAction : FailedAction;
                }
                catch
                {
                    result.Result = FailedAction;
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
        public decimal DraftVersion { get; set; }
        public decimal SubmittedVersion { get; set; }
        public string? Link { get; set; }
        public string Result { get; set; }
    }
}
