using ContactBook.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Server.Models;

namespace Server.Hub;

public class ListContactHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDb _db;

    public ListContactHub(ApplicationContextDb db)
    {
        _db = db;
    }

    public async Task GetContacts()
    {
        var contact = await _db.ContactLists.ToListAsync();
        await Clients.All.SendAsync("ReceiveContacts", contact);
    }
    public async Task AddContacts(string name, string lastName, string phone, string email, DateTimeOffset birthDay)
    {
        try
        {
            var dateOnly = DateOnly.FromDateTime(birthDay.DateTime);
            
            var contact = new ContactListModel
            {
                Name = name,
                LastName = lastName,
                Number = phone,
                Email = email,
                Birthday = dateOnly
            };
        
            await _db.ContactLists.AddAsync(contact);
            await _db.SaveChangesAsync();
        
            await Clients.All.SendAsync("SuccAddContact");
            Log.Information("Выполнено сохранение данных: " + contact);
        }
        catch (ArgumentNullException ex)
        {
            await Clients.All.SendAsync("ErrorAddContact", "Ошибка данных: " + ex.Message);
            Log.Error(ex, ex.Message);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await Clients.All.SendAsync("ErrorAddContact", ex.Message);
            Log.Error(ex, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            await Clients.All.SendAsync("ErrorAddContact", "Ошибка базы данных: " + ex.Message);
            Log.Error(ex, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.All.SendAsync("ErrorAddContact", "Ошибка работы с базой данных: " + ex.Message);
            Log.Error(ex, ex.Message);
        }
        catch (HubException ex)
        {
            Log.Error(ex, "Ошибка отправки сообщения через SignalR");
        }
        catch (Exception ex)
        {
            await Clients.All.SendAsync("ErrorAddContact", "Неизвестная ошибка: " + ex.Message);
            Log.Error(ex, ex.Message);
        }
    }
    public async Task DeleteContacts(int id)
    {
        var contact = await _db.ContactLists.FindAsync(id);

        if (contact != null)
        {
            _db.ContactLists.Remove(contact);
            await _db.SaveChangesAsync();
            await Clients.All.SendAsync("SuccDeleteContact", id);
        }
    }
    public async Task EditContact(int id, string name, string lastName, string number, string email, DateTimeOffset birthday)
    {
        var contact = await _db.ContactLists.FindAsync(id);
        
        if (contact != null)
        {
            var dateOnly = DateOnly.FromDateTime(birthday.DateTime);

            contact.Name = name;
            contact.LastName = lastName;
            contact.Number = number;
            contact.Email = email;
            contact.Birthday = dateOnly;

            try
            {
                _db.ContactLists.Update(contact);
                await _db.SaveChangesAsync();
                await Clients.All.SendAsync("SuccEditContact");
                Log.Information($"Контакт с ID {id} успешно обновлен.");
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Ошибка базы данных при обновлении контакта.");
                await Clients.All.SendAsync("ErrorEditContact", ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Неизвестная ошибка при обновлении контакта.");
                await Clients.All.SendAsync("ErrorEditContact", ex.Message);
            }
        }
    }
}