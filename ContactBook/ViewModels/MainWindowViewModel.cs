using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ContactBook.Service;

namespace ContactBook.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private object? _currentView;
    private string? _messageInfoBar;
    private string? _titleTextInfoBar;
    private string? _titleText;
    private int _statusInfoBar;
    private bool _isInfoBarVisible = false;

    private readonly ListContactViewModel _listContactViewModel;
    private readonly AddContactViewModel _addContactViewModel;

    public object CurrentView
    {
        get => _currentView!;
        set
        {
            SetProperty(ref _currentView, value);

            if (_currentView is IServerConnectionHandler newServerConnectionHandler)
                newServerConnectionHandler.ConnectToServer();
        }
    }

    public string MessageInfoBar
    {
        get => _messageInfoBar!;
        set => SetProperty(ref _messageInfoBar, value);
    }
    public string TitleTextInfoBar
    {
        get => _titleTextInfoBar!;
        set => SetProperty(ref _titleTextInfoBar, value);
    }
    public string TitleText
    {
        get => _titleText!;
        set => SetProperty(ref _titleText, value);
    }
    public int StatusInfoBar
    {
        get => _statusInfoBar;
        set => SetProperty(ref _statusInfoBar, value);
    }
    public bool IsInfoBarVisible
    {
        get => _isInfoBarVisible;
        set => SetProperty(ref _isInfoBarVisible, value);
    }

    public RelayCommand ListContactViewCommand { get; set; }
    public RelayCommand AddContactViewCommand { get; set; }
    
    public MainWindowViewModel()
    {
        _listContactViewModel = new ListContactViewModel(this);
        _addContactViewModel = new AddContactViewModel(this);
        
        CurrentView = _listContactViewModel;
        TitleText = "Список Контактов";

        ListContactViewCommand = new RelayCommand(() =>
        {
            CurrentView = _listContactViewModel;
            TitleText = "Список Контактов";
        });
        AddContactViewCommand = new RelayCommand(() =>
        {
            CurrentView = _addContactViewModel;
            TitleText = "Добавление Контактов";
        });
    }
    
    public async Task Notification(string title, string message, bool visibleInfoBar, int statusCode, bool timeLife)
    {
        TitleTextInfoBar = title;
        MessageInfoBar = message;
        IsInfoBarVisible = visibleInfoBar;
        StatusInfoBar = statusCode;

        if (timeLife)
        {
            await Task.Delay(3000);
            IsInfoBarVisible = false;
        }
    }
}