using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace ContactBook.ViewModels;

public class AddContactViewModel : BaseViewModel
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    
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

    public AsyncRelayCommand SaveNewContactCommand { get; set; }

    public AddContactViewModel(MainWindowViewModel mainWindowViewModel) : base("ListContactHub")
    {
        _mainWindowViewModel = mainWindowViewModel;
        SaveNewContactCommand = new AsyncRelayCommand(SaveNewContact);
    }

    public override async Task ConnectToServer()
    {
        try
        {
            await base.ConnectToServer();
            
            _hubConnection.On("SuccAddContact", () => {
                Dispatcher.UIThread.Post(() =>
                {
                    _mainWindowViewModel.CurrentView = new ListContactViewModel(_mainWindowViewModel);
                    Name = null;
                    LastName = null;
                    Number = null;
                    Email = null;
                    DateOfBirth = null;
                });
            });
            
            _hubConnection.On<string>("ErrorAddContact", response => {
                Dispatcher.UIThread.Post(() =>
                {
                    Log.Error(response);
                });
            });
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Не удалось подключиться к серверу.");
            await _mainWindowViewModel.Notification("Сервер", "Не удалось подключиться к серверу", true, 3, true);
        }
        catch (SocketException ex)
        {
            Log.Error(ex, "Сетевая ошибка при подключении к серверу.");
            await _mainWindowViewModel.Notification("Сеть", "При подключении к серверу произошла сетевая ошибка.", true, 3, true);
        }
        catch (HubException ex)
        {
            Log.Error(ex, "Ошибка подключения к SignalR хабу.");
            await _mainWindowViewModel.Notification("Сервер", "Возникла ошибка при подключении хаба SignalR.", true, 3, true);
        }
        catch (InvalidOperationException ex)
        {
            Log.Error(ex, "Ошибка в приложении.");
            await _mainWindowViewModel.Notification("Ошибка", "В приложении произошла ошибка. Пожалуйста, попробуйте еще раз.", true, 3, true);
        }
        catch (TimeoutException ex)
        {
            Log.Error(ex, "Соединение с сервером прервано.");
            await _mainWindowViewModel.Notification("Время ожидания", "Соединение с сервером прервано. Пожалуйста, попробуйте еще раз.", true, 3, true);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Произошла непредвиденная ошибка.");
            await _mainWindowViewModel.Notification("Ошибка", "Произошла непредвиденная ошибка.", true, 3, true);
        }
    }
    
    private async Task SaveNewContact()
    {
        await _hubConnection.InvokeAsync("AddContacts", Name, LastName, Number, Email, DateOfBirth);
    }
}