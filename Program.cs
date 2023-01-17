// See https://aka.ms/new-console-template for more information
using CasCap.Models;
using CasCap.Services;
using Google.Apis.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Net;

Console.WriteLine("Hello, World!");



var logger = new LoggerFactory().CreateLogger<GooglePhotosService>();

//new-up configuration options
var options = new GooglePhotosOptions
{
    User = "",
    ClientId = "",
    ClientSecret = "",
    Scopes = new[] { GooglePhotosScope.Access },
};

//new-up a single HttpClient
var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
var client = new HttpClient(handler) { BaseAddress = new Uri(options.BaseAddress) };

//new-up the GooglePhotosService and pass in the logger, options and HttpClient
var GPService = new GooglePhotosService(logger, Options.Create(options), client);

//attempt to log-in
if (!await GPService.LoginAsync())
    throw new Exception($"login failed!");

//get and list all albums
var photos = await GPService.GetMediaItemsByDateRangeAsync(DateTime.UnixEpoch,DateTime.Now).ToListAsync();
var path = @"D:\photos";
var downloaded = Directory.GetFiles(path).Select(p => Path.GetFileName(p)).ToList();
foreach (var ph in photos)
{
    if (downloaded.Contains(ph.filename)) 
        continue;
    var byt = await GPService.DownloadBytes(ph);
    if(byt is not null)
    {
        var file = new FileInfo($@"D:\photos\{ph.filename}");
        await File.WriteAllBytesAsync(file.FullName, byt);
        Console.WriteLine(ph.baseUrl);
    }
    Thread.Sleep(200);
}