using System.Net;
using AikiDataBuilder.Database;
using AikiDataBuilder.Model.SystemResponse;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace AikiDataBuilder.Services.Workers;

public abstract class Request
{
    public string Url { get; set; }
    protected HttpClient _httpClient;
    protected Dictionary<string, string> _headers = new Dictionary<string, string>();
    protected List<KeyValuePair<string, string>> UrlEncodedFormContent = new List<KeyValuePair<string, string>>();
    protected JsonContent _jsonContent;
    protected Dictionary<string, string> _queryParameters = new Dictionary<string, string>();
    protected SherwebDbContext _sherwebDBContext;

    public Request(
        HttpClient clientFactory,
        SherwebDbContext sherwebDBContext
        )
    {
        UrlEncodedFormContent = new List<KeyValuePair<string, string>>();
        _headers = new Dictionary<string, string>();
        _queryParameters = new Dictionary<string, string>();
        _httpClient = clientFactory;
        _sherwebDBContext = sherwebDBContext;
        
    }
    /// <summary>
    /// This function must be called after the 
    /// </summary>
    /// <param name="jsonContent"></param>
    /// <returns></returns>
    public abstract Task<OperationResult<JsonContent>> AddToDatabase(JsonContent jsonContent);

    public OperationResult<string> SetUrl(string url)
    {
        Url = url;
        return new OperationResult<string>()
        {
            Result = Url,
            Message = "URL set successfully",
            Exception = null,
            Status = OperationResultStatus.Success
        };
    }

    /// <summary>
    /// Simply sets the Bearer Token for when the request will be made
    /// </summary>
    /// <param name="bearerToken">The bearer token that will be used for the request</param>
    /// <returns>The bearer token that we sent</returns>

    public OperationResult<bool> SetBearerToken(string bearerToken)
    {
        const string headerKey = "Authorization";

        // Remove existing Authorization header if it exists
        if (_headers.ContainsKey(headerKey))
        {
            _headers.Remove(headerKey);
        }

        // Add the new token
        _headers.Add(headerKey, $"Bearer {bearerToken}");

        return new OperationResult<bool>(
            "Successfully set bearer token as Header", 
            OperationResultStatus.Success,
            null,
            true
        );
    }


    /// <summary>
    /// This will be transformed into Encoded Form Content For when the request is actually made
    /// </summary>
    /// <param name="formData">The data that will be encoded and placed into the request</param>
    /// <returns></returns>
    public OperationResult<List<KeyValuePair<string, string>>> SetFormUrlEncodedContent(
        Dictionary<string, string> formData)
    {
        var collection = new List<KeyValuePair<string, string>>();
        foreach (var item in formData)
        {
            collection.Add(new (item.Key, item.Value));
        }

        return new OperationResult<List<KeyValuePair<string, string>>>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully set form url encoded",
            Result = collection
        };
    }

    public OperationResult<JsonContent> SetJsonBody(JsonContent jsonContent)
    {
        _jsonContent = jsonContent;
        return new OperationResult<JsonContent>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully set json body",
            Result = jsonContent
        };
    }

    public OperationResult<Dictionary<string,string>> SetRouteVariables(Dictionary<string, string> routeVariables)
    {
        _queryParameters = routeVariables;
        return new OperationResult<Dictionary<string,string>>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully set route variables",
            Result = routeVariables
        };

    }

    public OperationResult<Dictionary<string, string>> SetHeaderVariables(Dictionary<string, string> headerVariables)
    {

        foreach (var header in headerVariables)
        {
            _headers.Add(header.Key, header.Value);
        }

        return new OperationResult<Dictionary<string, string>>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully set header variables",
            Result = headerVariables
        };
    }
    /// <summary>
    /// Is responsible to build the URL by replacing variables in the URL by real things
    /// </summary>
    /// <returns>The Built URL</returns>

    private OperationResult<string> BuildUrl()
    {
        string finalUrl = Url;
        try
        {
            foreach (var parameter in _queryParameters)
            {
                finalUrl = finalUrl.Replace(parameter.Key, parameter.Value);
            }
        }
        catch (Exception e)
        {
            return new OperationResult<string>()
            {
                Exception = e,
                Status = OperationResultStatus.Failed,
                Message = "Failed to place Query Parameters",
                Result = finalUrl
            };
        }

        return new OperationResult<string>()
        {
            Status = OperationResultStatus.Success,
            Message = "Successfully Built URL",
            Result = finalUrl
        };
    }


    public async virtual Task<OperationResult<JsonContent>> SendRequest(float timeout = 3000)
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        OperationResult<string> builtUrl = BuildUrl();
        if (builtUrl.Status != OperationResultStatus.Success)
            return await Task.FromResult(new OperationResult<JsonContent>
            {
                Status = OperationResultStatus.Failed,
                Message = builtUrl.Message + " Because of the previous problem, " +
                          "we will not be doing API calls to the endpoint. " +
                          "We will be skipping this call",
                Result = JsonContent.Create(builtUrl.Result),
                Exception = builtUrl.Exception
            });
        var request = new HttpRequestMessage(HttpMethod.Get, builtUrl.Result);
        if (!UrlEncodedFormContent.IsNullOrEmpty())
            request.Content = new FormUrlEncodedContent(UrlEncodedFormContent);
        if (_jsonContent != default)
            request.Content = request.Content;
        foreach (var header in _headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        HttpResponseMessage response = new HttpResponseMessage();
        try
        {
            response = _httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            // If it is simply unauthorized, we simply need to revise the toke
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return await Task.FromResult(new OperationResult<JsonContent>()
                {
                    Status = OperationResultStatus.PartialSuccess,
                    Message = "Encountered 401 Unauthorized Request",
                    Result = JsonContent.Create(response.Content.ReadAsStringAsync().Result),
                    Exception = e
                });
            //If it is anything else, well let's just say we'll need to skip
            return await Task.FromResult(new OperationResult<JsonContent>()
            {
                Status = OperationResultStatus.Failed,
                Message = $"Encountered {response.StatusCode} HTTP ERROR",
                Result = JsonContent.Create(response.Content.ReadAsStringAsync().Result),
                Exception = e
            });
        }
        catch (TaskCanceledException e)
        {
            return await Task.FromResult(new OperationResult<JsonContent>()
            {
                Status = OperationResultStatus.Failed,
                Message = $"The http request was canceled due to a Timeout Exception",
                Result = JsonContent.Create(response.Content.ReadAsStringAsync().Result),
                Exception = e
            });

        }

        var contentofResponse = response.Content.ReadAsStringAsync().Result;
        if (contentofResponse.IsNullOrEmpty())
        {
            return await Task.FromResult(new OperationResult<JsonContent>()
            {
                Status = OperationResultStatus.Failed,
                Message = $"Received Empty Response",
                Result = JsonContent.Create(JsonConvert.DeserializeObject(contentofResponse))
            });
        }

        return await Task.FromResult(new OperationResult<JsonContent>()
        {
            Status = OperationResultStatus.Success,
            Message = $"Received a valid Response",
            Result = JsonContent.Create(JsonConvert.DeserializeObject(contentofResponse))
        });
    }
}