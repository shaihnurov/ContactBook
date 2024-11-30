﻿using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ContactBook.Service;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace ContactBook.ViewModels;

public abstract class BaseViewModel : ObservableObject, IServerConnectionHandler
{
    protected HubConnection _hubConnection;
    
    private readonly string _serverUrl;
    private const int _maxRetryAttempts = 5;
    private const int _retryDelayInSeconds = 3;

    protected BaseViewModel(string serverUrl)
    {
        _serverUrl = serverUrl;
    }

    public virtual async Task ConnectToServer()
    {
        if (_hubConnection != null)
            return;

        int attempts = 0;

        while (attempts < _maxRetryAttempts)
        {
            try
            {
                _hubConnection = new HubConnectionBuilder().WithUrl($"https://localhost:7170/{_serverUrl}").WithAutomaticReconnect(
                [
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                ]).Build();

                await _hubConnection.StartAsync();
                Log.Information("Успешное подключение к серверу");
                return;
            }
            catch (HttpRequestException ex)
            {
                Log.Error($"HTTP Request Error: {ex.Message}. Attempt {attempts + 1} of {_retryDelayInSeconds}");
            }
            catch (SocketException ex)
            {
                Log.Error($"Socket Error: {ex.Message}");
                break;
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected Error: {ex.Message}");
                break;
            }

            attempts++;
            await Task.Delay(TimeSpan.FromSeconds(_retryDelayInSeconds));
        }
        
        Log.Warning("Ошибка подключения - использовано максимальное кол-во попыток");
    }
}