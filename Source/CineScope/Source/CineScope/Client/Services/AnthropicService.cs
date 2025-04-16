using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Diagnostics;
using Anthropic;
using CineScope.Shared.DTOs;
using Microsoft.JSInterop;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace CineScope.Client.Services
{
    public class AnthropicService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicService> _logger;
        private readonly IConfiguration _configuration;
        private Process? _mcpServerProcess;
        private bool _isServerRunning;
        private readonly SemaphoreSlim _serverLock = new(1, 1);
        private readonly string _mcpServerEndpoint;

        public AnthropicService(
            HttpClient httpClient,
            ILogger<AnthropicService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _mcpServerEndpoint = _configuration["MCPServer:Endpoint"] ?? "http://localhost:5000";
            _isServerRunning = false;
        }

        public async Task<string> GetMovieRecommendationAsync(string userQuery)
        {
            try
            {
                await EnsureMCPServerRunningAsync();

                var request = new
                {
                    Query = userQuery
                };

                var response = await _httpClient.PostAsJsonAsync($"{_mcpServerEndpoint}/api/mcp/query", request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<MCPResponse>();
                return result?.Response ?? "I apologize, but I couldn't process your request at this time.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting movie recommendation: {ex.Message}");
                return "I apologize, but there was an error processing your request. Please try again later.";
            }
        }

        private async Task EnsureMCPServerRunningAsync()
        {
            await _serverLock.WaitAsync();
            try
            {
                if (_isServerRunning && _mcpServerProcess?.HasExited == false)
                {
                    return;
                }

                await StartMCPServerAsync();
            }
            finally
            {
                _serverLock.Release();
            }
        }

        private async Task StartMCPServerAsync()
        {
            try
            {
                string mcpServerPath = "../CineScope.MCPServer/bin/Debug/net8.0/CineScope.MCPServer.dll";
                _logger.LogInformation($"Starting MCP server from {mcpServerPath}");

                _mcpServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = mcpServerPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _mcpServerProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogInformation($"MCP Server: {e.Data}");
                    }
                };

                _mcpServerProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        _logger.LogError($"MCP Server Error: {e.Data}");
                    }
                };

                bool started = _mcpServerProcess.Start();
                if (!started)
                {
                    throw new Exception("Failed to start MCP server process");
                }

                _mcpServerProcess.BeginOutputReadLine();
                _mcpServerProcess.BeginErrorReadLine();

                // Wait for the server to be ready
                await WaitForServerReadyAsync();
                _isServerRunning = true;

                _logger.LogInformation("MCP server started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting MCP server: {ex.Message}");
                throw;
            }
        }

        private async Task WaitForServerReadyAsync()
        {
            int maxAttempts = 30;
            int currentAttempt = 0;
            int delayMs = 1000;

            while (currentAttempt < maxAttempts)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{_mcpServerEndpoint}/health");
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("MCP server is ready");
                        return;
                    }
                }
                catch
                {
                    // Ignore exceptions during startup checks
                }

                await Task.Delay(delayMs);
                currentAttempt++;
            }

            throw new TimeoutException("MCP server failed to start within the expected timeframe");
        }

        public void Dispose()
        {
            try
            {
                if (_mcpServerProcess != null && !_mcpServerProcess.HasExited)
                {
                    _mcpServerProcess.Kill();
                    _mcpServerProcess.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error disposing MCP server process: {ex.Message}");
            }
        }

        private class MCPResponse
        {
            public string? Response { get; set; }
            public bool Success { get; set; }
            public string? Error { get; set; }
        }
    }
} 