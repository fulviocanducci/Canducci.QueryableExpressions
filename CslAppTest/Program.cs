using Canducci.QueryableExpressions.Filters.Extensions;
using CslAppTest.Models;
using Microsoft.EntityFrameworkCore;

DbContextOptionsBuilder<dbContext> optionsBuilder = new();
optionsBuilder.UseMySql("Server=127.0.0.1;Database=db;Uid=root;Pwd=senha;", ServerVersion.AutoDetect("Server=127.0.0.1;Database=db;Uid=root;Pwd=senha;"));
optionsBuilder.LogTo(Console.WriteLine).EnableSensitiveDataLogging();
dbContext db = new(optionsBuilder.Options);

//var res = db.Users.ApplySearch("M", SearchBy.Contains, x => x.Name, x => x.Gender).Where(c => c.Id > 10).ToList();

//foreach (var item in res)
//{
//    Console.WriteLine($"{item.Id} - {item.Name} - {item.Gender}");
//}

var items = new DynamicFilterBuilder()
        .AddEquals("Code", 1)
        .AddContains("Name", "M")        
        .Build();

//int? code = 1;
var res = db.Users
    .AsNoTracking()
    //.Where(c => c.Code == code)
    //.DynamicFilter("Name", "LUCAS", FilterOperator.StartsWith)
    //.DynamicFilter("CreatedAt", DateTime.Parse("27/11/2021"), FilterOperator.Equals)
    .DynamicFilters(items)
    //.DynamicFilter("Code", code)
    //.DynamicFilterNotNull("Code")
    .ToList();
//Console.WriteLine(res);
foreach (var item in res)
{
    Console.WriteLine($"{item.Id} - {item.Name} - {item.Gender}");
}
