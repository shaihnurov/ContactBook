using System;

namespace ContactBook.Models;

public class ContactListModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? Number { get; set; }
    public string? Email { get; set; }
    public DateOnly? Birthday { get; set; }
}