using System;
using System.Threading.Tasks;

namespace ContactBook.Service;

public interface IDialogService
{
    public Task<string?> ShowDialogAsync(int id, string? name, string? lastName, string? phone, string? email, DateOnly? birthday);
}