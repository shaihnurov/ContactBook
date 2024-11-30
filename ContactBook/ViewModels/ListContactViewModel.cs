using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using ActiproSoftware.UI.Avalonia.Data;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ContactBook.Models;
using ContactBook.Service;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace ContactBook.ViewModels;

public class ListContactViewModel : BaseViewModel
{
    private bool _isLoad = true;
    private bool _isSuccConnect = false;
    private string _searchText = string.Empty;
    private ObservableCollection<ContactListModel>? _contactList;
    private ICollectionView<ContactListModel>? _contactCollectionView;
    
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly DialogService _dialogService;
    
    public bool IsLoad
    {
        get => _isLoad;
        set => SetProperty(ref _isLoad, value);
    }
    public bool IsSuccConnect
    {
        get => _isSuccConnect;
        set => SetProperty(ref _isSuccConnect, value);
    }
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            _contactCollectionView?.Refresh();
        }
    }
    public ObservableCollection<ContactListModel>? ContactList
    {
        get => _contactList;
        set => SetProperty(ref _contactList, value);
    }
    public ICollectionView<ContactListModel>? ContactCollectionView
    {
        get => _contactCollectionView;
        set => SetProperty(ref _contactCollectionView, value);
    }

    public AsyncRelayCommand<object> EditContactCommand { get; set; }
    public AsyncRelayCommand<object> DeleteContactCommand { get; set; }

    public ListContactViewModel(MainWindowViewModel mainWindowViewModel) : base("ListContactHub")
    {
        _mainWindowViewModel = mainWindowViewModel;
        _dialogService = new DialogService();
        ContactList = [];
        ContactCollectionView = new CollectionView<ContactListModel>(ContactList)
        {
            Filter = ContactFilter
        };

        EditContactCommand = new AsyncRelayCommand<object>(EditContact!);
        DeleteContactCommand = new AsyncRelayCommand<object>(DeleteContact!);
    }

    public override async Task ConnectToServer()
    {
        try
        {
            await base.ConnectToServer();
            
            _hubConnection.On<ObservableCollection<ContactListModel>>("ReceiveContacts", contact => {
                Dispatcher.UIThread.Post(() =>
                {
                    ContactList!.Clear();
                    foreach (var item in contact)
                    {
                        ContactList.Add(item);
                    }
                    ContactCollectionView?.Refresh();
                });
            });
            _hubConnection.On<int>("SuccDeleteContact", id => {
                Dispatcher.UIThread.Post(() =>
                {
                    var contactToRemove = ContactList!.FirstOrDefault(c => c.Id == id);
                    if (contactToRemove != null)
                    {
                        ContactList!.Remove(contactToRemove);
                    }
                });
            });
            _hubConnection.On<ContactListModel>("SuccEditContact", updatedContact => {
                Dispatcher.UIThread.Post(() =>
                {
                    var contact = ContactList!.FirstOrDefault(c => c.Id == updatedContact.Id);
                    if (contact != null)
                    {
                        contact.Name = updatedContact.Name;
                        contact.LastName = updatedContact.LastName;
                        contact.Number = updatedContact.Number;
                        contact.Email = updatedContact.Email;
                        contact.Birthday = updatedContact.Birthday;
                    }
                });
            });
            _hubConnection.On<string>("ErrorEditContact", response => {
                Dispatcher.UIThread.Post(() =>
                {
                    Log.Error(response);
                });
            });
            
            IsLoad = false;
            IsSuccConnect = true;
            await _hubConnection.InvokeAsync("GetContacts");
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
    private async Task DeleteContact(object parameter)
    {
        if (parameter is ContactListModel currentContact)
        {
            await _hubConnection.InvokeAsync("DeleteContacts", currentContact.Id);
        }
    }
    private async Task EditContact(object parameter)
    {
        if (parameter is ContactListModel currentContact)
        {
            await _dialogService.ShowDialogAsync(currentContact.Id, currentContact.Name, currentContact.LastName, currentContact.Number, currentContact.Email, currentContact.Birthday);
        }
    }
    private bool ContactFilter(object? obj)
    {
        if (obj is not ContactListModel contact)
            return false;

                var searchWords = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return searchWords.All(word => (!string.IsNullOrWhiteSpace(contact.Name)) && contact.Name!.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                                       (!string.IsNullOrWhiteSpace(contact.LastName)) && contact.LastName!.Contains(word, StringComparison.OrdinalIgnoreCase) ||
                                       (!string.IsNullOrWhiteSpace(contact.Email)) && contact.Email!.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}