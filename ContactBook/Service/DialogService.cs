using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ContactBook.ViewModels;
using ContactBook.Views;

namespace ContactBook.Service;

public class DialogService : IDialogService
{
    public async Task<string?> ShowDialogAsync(int id, string? name, string? lastName, string? phone, string? email, DateOnly? birthday)
    {
        var dialogWindow = new DialogWindow();
        dialogWindow.DataContext = new DialogWindowViewModel(dialogWindow, id, name, lastName, phone, email, birthday);
        
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return await dialogWindow.ShowDialog<string>(desktop.MainWindow);
        }

        return null;
    }
}