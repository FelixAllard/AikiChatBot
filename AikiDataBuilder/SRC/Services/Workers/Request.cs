using AikiDataBuilder.Model.SystemResponse;

namespace AikiDataBuilder.Services.Workers;

public abstract class Request
{
    public string Url { get; set; }
    protected HttpClient _httpClient;
    protected Dictionary<string, string> _headers;
    protected List<KeyValuePair<string, string>> UrlEncodedFormContent;
    protected JsonContent _jsonContent;
    protected Dictionary<string, string> _queryParameters;

    public Request(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient();
        
    }

    /// <summary>
    /// Simply sets the Bearer Token for when the request will be made
    /// </summary>
    /// <param name="bearerToken">The bearer token that will be used for the request</param>
    /// <returns>The bearer token that we sent</returns>

    public OperationResult<bool> SetBearerToken(string bearerToken)
    {
        _headers.Add("Authorization", $"Bearer {bearerToken}");
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


    public virtual Task<OperationResult<JsonContent>> SendRequest(Request request, float timeout = 3000)
    {
        throw new NotImplementedException();
    }
    
}