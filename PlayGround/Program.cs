using System.Net.Http.Headers;
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

        Console.WriteLine("Get root items...");
        var rootItems = await graphClient.Me.Drive.Root.Children.Request().GetAsync();
        Console.WriteLine("Get Pixiv dir...");
        var pixivDir = rootItems.CurrentPage.First(i => i.Name == "Pixiv");
        Console.WriteLine("Get Pixiv dir items...");
        var dirItems = await graphClient.Me.Drive.Items[pixivDir.Id].Children.Request().GetAsync();

        Console.WriteLine("holy shit");
        return;
    }
}