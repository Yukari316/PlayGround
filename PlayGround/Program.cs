using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Graph;

namespace PlayGround;

public static class Program
{
    private static HttpClient client = new();

    public static async Task Main()
    {
        var tokenref = new TokenRefresh();
        (bool success, Exception err) = await tokenref.RefreshToken();

        var graphClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(
                request =>
                {
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", tokenref.TokenFile.AccessToken);
                    return Task.CompletedTask;
                }));

        Console.WriteLine("Get Pixiv dir...");
        var picItem = await graphClient.Me.Drive.Root.ItemWithPath("/Pixiv/20977495_p0.jpg").Request().GetAsync();

        var dlInfo = ((JsonElement) picItem.AdditionalData["@microsoft.graph.downloadUrl"]).GetString();

        Console.WriteLine(dlInfo);

        await Task.Delay(-1);
    }
}