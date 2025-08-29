using Microsoft.Extensions.Http;
using Nextech.Hn.Api.Adapters;
using Nextech.Hn.Api.Config;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HackerNews options
builder.Services.Configure<HackerNewsOptions>(
    builder.Configuration.GetSection("HackerNews"));

// Configure HttpClient with Polly retry policy
builder.Services.AddHttpClient<IHackerNewsClient, HackerNewsClient>()
    .AddPolicyHandler(GetRetryPolicy());

// Register application services  
builder.Services.AddScoped<IHackerNewsClient, HackerNewsClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Handles HttpRequestException, 5XX and 408 Timeout responses
        .Or<TaskCanceledException>() // Handle timeout via cancellation
        .WaitAndRetryAsync(
            retryCount: 2,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)), // Exponential backoff with jitter
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan}s delay");
            });
}

