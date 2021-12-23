using GenFu;
using StackExchange.Redis;
// ReSharper disable AccessToStaticMemberViaDerivedType

HttpClient client = new HttpClient();
var resp = await client.GetAsync("https://raw.githubusercontent.com/ngosang/trackerslist/master/trackers_best.txt");
var trakers = (await resp.Content.ReadAsStringAsync()).Replace("\n\n",",")[..^1];

Console.WriteLine(trakers);

Console.ReadKey();