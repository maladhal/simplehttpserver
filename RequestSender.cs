
using System.Text;
using System.Text.Json;

public static class RequestSender
{    public static async Task<HttpResponseMessage> PostWorkflowAsync(string url, string workflowName, string workflowRunId, string informationSubject)
    {
        using (var httpClient = new HttpClient())
        {
            var payload = new
            {
                workflow_name = workflowName,
                workflow_runid = workflowRunId,
                information_subject = informationSubject
            };

            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            return response;
        }
    }
}