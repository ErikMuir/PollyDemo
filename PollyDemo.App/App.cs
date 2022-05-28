namespace PollyDemo.App;

public class App
{
    private static readonly AppLogger _logger = new AppLogger();
    private readonly HttpClient _httpClient;

    public App(HttpClient client)
    {
        _httpClient = client;
        _httpClient.GetAsync("/setup").Wait();
        _logger.Clear();
    }

    #region Demo stuff
    private static int _exceptionCount = 0;
    private async Task DrillBabyDrill()
    {
        // utilize all 4 bulkhead slots and 2 queue slots
        // then one more call to see the bulkhead exception
        for (var i = 0; i < 7; i++)
        {
            await Task.Delay(50);
            GetResponse("/").GetAwaiter();
        }

        // then wait for a slot to free up
        await Task.Delay(500);
    }
    #endregion

    public async Task Run(string path)
    {
        // await DrillBabyDrill();

        _logger.LogRequest();

        var response = await GetResponse(path);

        _logger.LogResponse(response);
    }

    private async Task<HttpResponseMessage?> GetResponse(string path)
    {
        return await _httpClient.GetAsync(path);
    }
}
