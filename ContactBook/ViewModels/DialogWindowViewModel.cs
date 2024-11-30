using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace ContactBook.ViewModels;

public class DialogWindowViewModel : BaseViewModel
{
    private readonly Window _dialogWindow;
    
    private int _id;
    private string? _name;
    private string? _lastName;
    private string? _number;
    private string? _email;
    private DateTimeOffset? _dateOfBirth;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    public string? LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }
    public string? Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
    public DateTimeOffset? DateOfBirth
    {
        get => _dateOfBirth;
        set => SetProperty(ref _dateOfBirth, value);
    }
    
    public AsyncRelayCommand SaveEditContactCommand { get; set; }
    public RelayCommand CancelCommand { get; set; }
    
    public DialogWindowViewModel(Window window, int id, string? name, string? lastName, string? phone, string? email, DateOnly? birthday) : base("ListContactHub")
    {
        _dialogWindow = window;
        LoadData(id, name, lastName, phone, email, birthday);
        
        CancelCommand = new RelayCommand(Cancel);
        SaveEditContactCommand = new AsyncRelayCommand(SaveEditContact);
    }

    public override async Task ConnectToServer()
    {
        try
        {
            await base.ConnectToServer();
            
            _hubConnection.On("SuccEditContact", () => {
                Dispatcher.UIThread.Post(() =>
                {
                    _hubConnection.InvokeAsync("GetContacts");
                    _dialogWindow.Close();
                });
            });
            
            _hubConnection.On<string>("ErrorEditContact", response => {
                Dispatcher.UIThread.Post(() =>
                {
                    Log.Error(response);
                });
            });
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Не удалось подключиться к серверу.");
        }
        catch (SocketException ex)
        {
            Log.Error(ex, "Сетевая ошибка при подключении к серверу.");
        }
        catch (HubException ex)
        {
            Log.Error(ex, "Ошибка подключения к SignalR хабу.");
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Ошибка в приложении.");
        }
        catch (TimeoutException ex)
        {
            Log.Error(ex, "Соединение с сервером прервано.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Произошла непредвиденная ошибка.");
        }
    }
    private void LoadData(int id, string? name, string? lastName, string? phone, string? email, DateOnly? birthday)
    {
        _id = id;
        Name = name;
        LastName = lastName;
        Number = phone;
        Email = email;
        DateOfBirth = ConvertDateOnlyToDateTimeOffset(birthday);
    }
    private static DateTimeOffset? ConvertDateOnlyToDateTimeOffset(DateOnly? dateOnly)
    {
        if (dateOnly == null)
            return null;

        var dateTime = dateOnly.Value.ToDateTime(TimeOnly.MinValue);
        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }
    private async Task SaveEditContact()
    {
        await ConnectToServer();
        await _hubConnection.InvokeAsync("EditContact", _id, Name, LastName, Number, Email, DateOfBirth);
    }
    private void Cancel()
    {
        _dialogWindow.Close();
    }
}