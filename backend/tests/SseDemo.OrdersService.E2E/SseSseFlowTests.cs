// Copyright (c) SseDemo. All rights reserved.
namespace SseDemo.OrdersService.E2E;

using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

/// <summary>
/// End-to-end test validating creating an order results in SSE event.
/// </summary>
public class SseSseFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SseSseFlowTests"/> class with a test web host.
    /// </summary>
    /// <param name="factory">Host factory.</param>
    public SseSseFlowTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(_ => { });
    }

    /// <summary>
    /// Creating an order via REST should emit an order-created SSE that we receive on the open stream.
    /// </summary>
    [Fact(Timeout = 15000)]
    public async Task CreateOrder_ShouldEmitOrderCreatedEvent()
    {
        using var client = this.factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
        });

        using var sseRequest = new HttpRequestMessage(HttpMethod.Get, "/sse/stream");
        sseRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        var sseResponse = await client.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead);
        sseResponse.EnsureSuccessStatusCode();
        var stream = await sseResponse.Content.ReadAsStreamAsync();

        var receivedTask = Task.Run(async () =>
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var buffer = new StringBuilder();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line))
                {
                    var evt = buffer.ToString();
                    if (evt.Contains("event:order-created"))
                    {
                        return evt;
                    }

                    buffer.Clear();
                }
                else
                {
                    buffer.AppendLine(line);
                }
            }

            return string.Empty;
        });

        var body = JsonSerializer.Serialize(new { customerName = "E2E", totalAmount = 1.23m });
        var createResp = await client.PostAsync("/api/orders", new StringContent(body, Encoding.UTF8, "application/json"));
        createResp.EnsureSuccessStatusCode();

        var completed = await Task.WhenAny(receivedTask, Task.Delay(8000));
        Assert.True(completed == receivedTask, "Did not receive SSE event in time");
        var payload = await receivedTask;
        Assert.Contains("order-created", payload);
        Assert.Contains("E2E", payload);
    }
}
