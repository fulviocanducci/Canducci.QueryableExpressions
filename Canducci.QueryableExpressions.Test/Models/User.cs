using System;
using System.Collections.Generic;

namespace Canducci.QueryableExpressions.Test.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Gender { get; set; } = string.Empty;
        public int? Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
    }
}