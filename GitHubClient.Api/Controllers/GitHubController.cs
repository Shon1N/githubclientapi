using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using GitHubClient.Core.Models;

namespace GitHubClient.Api.Controllers
{
    [ApiController]
    public class GitHubController : ControllerBase
    {
        private HttpClient Client { get; }
        public GitHubController(HttpClient client)
        {
            Client = client;
        }

        [Route("api/[controller]/GetCommitsAsync")]
        [HttpGet]
        public async Task<IActionResult> GetCommitsAsync(string userNameAndRepoName)//"shon1n/intric-node-api-boilerplate"
        {
            string endPoint = $"repos/{userNameAndRepoName}/commits";
            var ResponseModel = new ResponseModel<List<CommitModel>>();
            var CommitModels = new List<CommitModel>();
            try
            {
                var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "").Trim();
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                Client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                HttpResponseMessage response = await Client.GetAsync(endPoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(data);

                    foreach(var commit in json)
                    {
                        var commitModel = new CommitModel();
                        commitModel.CommitSha = commit["sha"];
                        commitModel.CommitMessage = commit["commit"]["message"];
                        commitModel.CommitterName = commit["commit"]["author"]["name"];
                        commitModel.Email = commit["commit"]["author"]["email"];
                        commitModel.Date = commit["commit"]["author"]["date"];
                        commitModel.TreeUrl = commit["commit"]["tree"]["url"];
                        CommitModels.Add(commitModel);
                    }
                    ResponseModel.StatusCode = (int)HttpStatusCode.OK;
                    ResponseModel.Resonse = CommitModels;
                    ResponseModel.Message = "Request processed successfully";
                }
                else
                {
                    ResponseModel.StatusCode = (int)HttpStatusCode.BadRequest;
                    ResponseModel.Message = "Failed";
                }

                return Ok(ResponseModel);
            }
            catch(Exception ex)
            {
                ResponseModel.StatusCode = (int)HttpStatusCode.InternalServerError;
                ResponseModel.Message = ex.Message;
                return Ok(ResponseModel);
            }
        }

        [Route("api/[controller]/GetCommitByShaAsync")]
        [HttpGet]
        public async Task<IActionResult> GetCommitByShaAsync(string userNameAndRepoName, string sha)//"shon1n/intric-node-api-boilerplate"
        {
            string endPoint = $"repos/{userNameAndRepoName}/commits/{sha}";
            var ResponseModel = new ResponseModel<List<CommitDetailModel>>();
            ResponseModel.Resonse = new List<CommitDetailModel>();
            try
            {
                var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "").Trim();

                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                Client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                HttpResponseMessage response = await Client.GetAsync(endPoint);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    dynamic json = JsonConvert.DeserializeObject(data);
                    foreach (var file in json["files"])
                    {
                        var commitFileModel = new CommitDetailModel();
                        commitFileModel.FileName = file["filename"];
                        commitFileModel.Status = file["status"];
                        commitFileModel.Patch = file["patch"];
                        commitFileModel.AdditionCount = file["additions"];
                        commitFileModel.DeletionCount = file["deletions"];
                        commitFileModel.ChangeCount = file["changes"];
                        ResponseModel.Resonse.Add(commitFileModel);
                    }

                    //CommitDetailModel.CommitFileModels.Add()                      
                    ResponseModel.StatusCode = (int)HttpStatusCode.OK;
                    ResponseModel.Message = "Request processed successfully";
                }
                else
                {
                    ResponseModel.StatusCode = (int)HttpStatusCode.BadRequest;
                    ResponseModel.Message = "Failed";
                }
                return Ok(ResponseModel);
            }
            catch (Exception ex)
            {
                ResponseModel.StatusCode = (int)HttpStatusCode.InternalServerError;
                ResponseModel.Message = ex.Message;
                return Ok(ResponseModel);
            }
        }

    }
}
