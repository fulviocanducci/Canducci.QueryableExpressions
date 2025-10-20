
using System;
using System.Collections.Generic;

namespace CslAppTest.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; } = string.Empty;
    public int? Code { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}

public class UserView
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Gender { get; set; } = string.Empty;    
}