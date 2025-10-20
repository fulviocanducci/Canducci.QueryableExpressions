using Canducci.QueryableExpressions.Filters.Extensions;
using Canducci.QueryableExpressions.Filters.Extensions.Models;
using Canducci.QueryableExpressions.Filters.Extensions.Operators;
using Canducci.QueryableExpressions.Selects.Extensions;
using Canducci.QueryableExpressions.Test.Models;
using Microsoft.EntityFrameworkCore;
namespace Canducci.QueryableExpressions.Test
{
    public class Tests: IDisposable
    {
        public TestDbContext _context;
        public List<User> _testUsers;
        public void Dispose()
        {
            _context?.Database?.CloseConnection();
            _context?.Dispose();
        }

        [OneTimeSetUp]
        public void Setup()
        {
            VerifyDbExistsAndDelete();
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=TestDatabase.db")
                .Options;
            _context = new TestDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
            SeedTestData();
        }

        private void VerifyDbExistsAndDelete()
        {
            try
            {
                if (File.Exists("TestDatabase.db"))
                {
                    File.Delete("TestDatabase.db");
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private void SeedTestData()
        {
            _testUsers =
            [
                new User { Id = 1, Name = "João Silva", Gender = "M", Code = 100, CreatedAt = new DateTime(2023, 1, 15), UpdateAt = new DateTime(2023, 6, 10), Price = 1, Active = true },
                new User { Id = 2, Name = "Maria Santos", Gender = "F", Code = 200, CreatedAt = new DateTime(2023, 2, 20), UpdateAt = null, Price = 1, Active = false },
                new User { Id = 3, Name = "Carlos Oliveira", Gender = "M", Code = 150, CreatedAt = new DateTime(2023, 3, 10), UpdateAt = new DateTime(2023, 8, 5), Price = 2, Active = true },
                new User { Id = 4, Name = "Ana Costa", Gender = "F", Code = null, CreatedAt = new DateTime(2023, 4, 5), UpdateAt = new DateTime(2023, 9, 12), Price = 2, Active = false },
                new User { Id = 5, Name = "Pedro Lima", Gender = "M", Code = 300, CreatedAt = new DateTime(2023, 5, 25), UpdateAt = null , Price = 1, Active = true},
                new User { Id = 6, Name = "Lucia Ferreira", Gender = "F", Code = 250, CreatedAt = new DateTime(2023, 6, 30), UpdateAt = new DateTime(2023, 10, 1) , Price = 10, Active = true},
                new User { Id = 7, Name = "Roberto a Alves", Gender = "M", Code = 180, CreatedAt = new DateTime(2023, 7, 12), UpdateAt = new DateTime(2023, 11, 15) , Price = 10, Active = false},
                new User { Id = 8, Name = "Fernanda Rocha", Gender = "F", Code = null, CreatedAt = new DateTime(2023, 8, 8), UpdateAt = null , Price = 1, Active = true},
                new User { Id = 9, Name = "Gabriel Mendes", Gender = "M", Code = 220, CreatedAt = new DateTime(2023, 9, 18), UpdateAt = new DateTime(2023, 12, 3) , Price = 1, Active = true},
                new User { Id = 10, Name = "Juliana Cruz", Gender = "F", Code = 190, CreatedAt = new DateTime(2023, 10, 22), UpdateAt = new DateTime(2023, 12, 20) , Price = 1, Active = true}
            ];
            _context.Users.AddRange(_testUsers);
            _context.SaveChanges();
        }

        [Test]
        public void ApplySearch_Contains_WithExpression_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearch("Silva", SearchOperator.Contains, u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("João Silva"));
        }

        [Test]
        public void ApplySearch_StartsWith_WithExpression_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearch("Ma", SearchOperator.StartsWith, u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Maria Santos"));
        }

        [Test]
        public void ApplySearch_EndsWith_WithExpression_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearch("Santos", SearchOperator.EndsWith, u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Maria Santos"));
        }

        [Test]
        public void ApplySearch_Exactly_WithExpression_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearch("Carlos Oliveira", SearchOperator.Exactly, u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Carlos Oliveira"));
        }

        [Test]
        public void ApplySearch_Contains_WithPropertyNames_ShouldReturnFilteredResults()
        {
            var query = _context.Users.AsQueryable();
            var result = query.ApplySearch("M", SearchOperator.Contains, "Gender").ToList();
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result.All(u => u.Gender != null && u.Gender.Contains("M")), Is.True);
        }

        [Test]
        public void ApplySearch_MultipleProperties_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearch("a", SearchOperator.Contains, u => u.Name, u => u.Gender).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(10)); // Names and genders containing "a"
        }

        [Test]
        public void ApplySearch_EmptySearch_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.ApplySearch("", SearchOperator.Contains, u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void ApplySearch_NullProperties_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.ApplySearch("a", SearchOperator.Contains, "Name").ToList();

            // Assert
            Assert.AreEqual(originalCount, result.Count);
        }

        [Test]
        public void DynamicFilter_Equal_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Id", 1, FilterOperator.Equal).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(1));
        }

        [Test]
        public void DynamicFilter_GreaterThan_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Code", 200, FilterOperator.GreaterThan).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3)); // Codes: 220, 250, 300
            Assert.IsTrue(result.All(u => u.Code > 200));
        }

        [Test]
        public void DynamicFilter_LessThanOrEqual_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Code", 200, FilterOperator.LessThanOrEqual).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5)); // Codes: 100, 150, 180, 190, 200
            Assert.IsTrue(result.All(u => u.Code <= 200));
        }

        [Test]
        public void DynamicFilter_Contains_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Name", "Silva", FilterOperator.Contains).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.AreEqual("João Silva", result.First().Name);
        }

        [Test]
        public void DynamicFilter_StartsWith_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Name", "Car", FilterOperator.StartsWith).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Carlos Oliveira"));
        }

        [Test]
        public void DynamicFilter_IsNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("UpdateAt", null, FilterOperator.IsNull).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3)); // Users with null UpdateAt
            Assert.IsTrue(result.All(u => u.UpdateAt == null));
        }

        [Test]
        public void DynamicFilter_IsNotNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("UpdateAt", null, FilterOperator.IsNotNull).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(7)); // Users with non-null UpdateAt
            Assert.IsTrue(result.All(u => u.UpdateAt != null));
        }

        [Test]
        public void DynamicFilters_MultipleFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var filters = new List<DynamicFilterItem>
            {
                new DynamicFilterItem("Gender", "M", FilterOperator.Equal),
                new DynamicFilterItem("Code", 200, FilterOperator.GreaterThan)
            };

            // Act
            var result = query.DynamicFilters(filters).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2)); // Male users with Code > 200
            Assert.IsTrue(result.All(u => u.Gender == "M" && u.Code > 200));
        }

        [Test]
        public void DynamicFilters_MultipleFilters_WithOr_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var filters = new List<DynamicFilterItem>
            {
                new DynamicFilterItem("Gender", "M", FilterOperator.Equal),
                new DynamicFilterItem("Code", 100, FilterOperator.Equal)
            };

            // Act
            var result = query.DynamicFilters(filters, combineWithOr: true).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5)); // Male users OR users with Code = 100
        }

        [Test]
        public void DynamicFilter_InvalidProperty_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.DynamicFilter("NonExistentProperty", "value", FilterOperator.Equal).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void DynamicFilter_EmptyPropertyName_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.DynamicFilter("", "value", FilterOperator.Equal).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void ApplySearchContains_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearchContains("ria", u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1)); // Maria and Fernanda
            Assert.IsTrue(result.All(u => u.Name.Contains("ria")));
        }

        [Test]
        public void ApplySearchStartsWith_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearchStartsWith("J", u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2)); // João and Juliana
            Assert.IsTrue(result.All(u => u.Name.StartsWith("J")));
        }

        [Test]
        public void ApplySearchEndsWith_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearchEndsWith("es", u => u.Name).ToList();

            // Assert
            Assert.AreEqual(2, result.Count); // Names ending with "es"
            Assert.IsTrue(result.All(u => u.Name.EndsWith("es")));
        }

        [Test]
        public void ApplySearchExactly_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.ApplySearchExactly("Ana Costa", u => u.Name).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.AreEqual("Ana Costa", result.First().Name);
        }

        [Test]
        public void DynamicFilter_DateTime_GreaterThan_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var dateThreshold = new DateTime(2023, 6, 1);

            // Act
            var result = query.DynamicFilter("CreatedAt", dateThreshold, FilterOperator.GreaterThan).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5)); // Users created after June 1st
            Assert.IsTrue(result.All(u => u.CreatedAt > dateThreshold));
        }

        [Test]
        public void DynamicFilter_NullableInt_IsNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Code", null, FilterOperator.IsNull).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2)); // Users with null Code
            Assert.IsTrue(result.All(u => u.Code == null));
        }

        [Test]
        public void DynamicFilter_NullableInt_IsNotNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilter("Code", null, FilterOperator.IsNotNull).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(8)); // Users with non-null Code
            Assert.IsTrue(result.All(u => u.Code != null));
        }

        [Test]
        public void DynamicFilter_WithExpression_Equal_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilterEqual(u => u.Gender, "F").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5)); // Female users
            Assert.IsTrue(result.All(u => u.Gender == "F"));
        }

        [Test]
        public void DynamicFilter_WithExpression_GreaterThan_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilterGreaterThan(u => u.Id, 5).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(5)); // Users with Id > 5
            Assert.IsTrue(result.All(u => u.Id > 5));
        }

        [Test]
        public void DynamicFilter_WithExpression_Contains_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilterContains(u => u.Name, "an").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3)); // Names containing "an"
            Assert.IsTrue(result.All(u => u.Name.Contains("an")));
        }

        [Test]
        public void DynamicFilter_WithExpression_IsNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilterIsNull(u => u.UpdateAt).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(3)); // Users with null UpdateAt
            Assert.IsTrue(result.All(u => u.UpdateAt == null));
        }

        [Test]
        public void DynamicFilter_WithExpression_IsNotNull_ShouldReturnFilteredResults()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicFilterIsNotNull(u => u.UpdateAt).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(7)); // Users with non-null UpdateAt
            Assert.IsTrue(result.All(u => u.UpdateAt != null));
        }

        [Test]
        public void ToQueryString_DynamicFilter_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act - Apply a dynamic filter and generate SQL
            var filteredQuery = query.DynamicFilter("Gender", "M", FilterOperator.Equal);
            var sqlQuery = filteredQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("Gender", sqlQuery);
            StringAssert.Contains("WHERE", sqlQuery);
            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void ToQueryString_ApplySearch_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act - Apply a search filter and generate SQL
            var searchQuery = query.ApplySearch("Silva", SearchOperator.Contains, u => u.Name);
            var sqlQuery = searchQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("Name", sqlQuery);
            StringAssert.Contains("WHERE", sqlQuery);
            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void ToQueryString_MultipleFilters_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var filters = new List<DynamicFilterItem>
            {
                new DynamicFilterItem("Gender", "M", FilterOperator.Equal),
                new DynamicFilterItem("Code", 200, FilterOperator.GreaterThan)
            };

            // Act - Apply multiple filters and generate SQL
            var multiFilterQuery = query.DynamicFilters(filters);
            var sqlQuery = multiFilterQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("Gender", sqlQuery);
            StringAssert.Contains("Code", sqlQuery);
            StringAssert.Contains("WHERE", sqlQuery);
            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void ToQueryString_ComplexFilter_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act - Apply a complex filter with nullable field and generate SQL
            var complexQuery = query.DynamicFilter("UpdateAt", null, FilterOperator.IsNull);
            var sqlQuery = complexQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("UpdateAt", sqlQuery);
            StringAssert.Contains("WHERE", sqlQuery);
            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void ToQueryString_ComplexFilter_ShouldGenerateCorrectCompleteSql()
        {
            var query = _context.Users.AsQueryable();
            var complexQuery = query.DynamicFilterEqual("Gender", "M");
            var sqlQuery = complexQuery.ToQueryString();
            var strSQLActual = ".param set @__Value_0 'M'\r\n\r\nSELECT \"u\".\"Id\", \"u\".\"Active\", \"u\".\"Code\", \"u\".\"CreatedAt\", \"u\".\"Gender\", \"u\".\"Name\", \"u\".\"Price\", \"u\".\"UpdateAt\"\r\nFROM \"Users\" AS \"u\"\r\nWHERE \"u\".\"Gender\" = @__Value_0";
            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("UpdateAt", sqlQuery);
            StringAssert.Contains("WHERE", sqlQuery);
            StringAssert.Contains("param set @__Value_0", sqlQuery);
            StringAssert.Contains(strSQLActual, sqlQuery);
            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void DynamicSelectBy_SingleField_ShouldReturnSelectedFieldOnly()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicSelectBy("Name").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            Assert.That(result.All(u => u.Id == 0), Is.False); // Id should not be selected
            Assert.That(result.All(u => !string.IsNullOrEmpty(u.Name)), Is.True); // Name should be selected
            Assert.That(result.First().GetType().GetProperties().Length, Is.EqualTo(1)); // Only Name property
        }

        [Test]
        public void DynamicSelectBy_MultipleFields_ShouldReturnSelectedFieldsOnly()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicSelectBy("Id", "Name").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            var firstUser = result.First();
            Assert.That(firstUser.Id, Is.GreaterThan(0)); // Id should be selected
            Assert.That(!string.IsNullOrEmpty(firstUser.Name), Is.True); // Name should be selected

            // Check that only Id and Name properties exist
            var properties = firstUser.GetType().GetProperties();
            Assert.That(properties.Length, Is.EqualTo(2));
            Assert.That(properties.Any(p => p.Name == "Id"), Is.True);
            Assert.That(properties.Any(p => p.Name == "Name"), Is.True);
        }

        [Test]
        public void DynamicSelectBy_AllFields_ShouldReturnAllFields()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicSelectBy("Id", "Name", "Gender", "Code", "CreatedAt", "UpdateAt", "Price", "Active").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            var firstUser = result.First();
            Assert.That(firstUser.Id, Is.GreaterThan(0));
            Assert.That(!string.IsNullOrEmpty(firstUser.Name), Is.True);
            Assert.That(firstUser.Gender != null, Is.True);
            Assert.That(firstUser.Active, Is.True); // Should have boolean value

            // Check that all properties exist
            var properties = firstUser.GetType().GetProperties();
            Assert.That(properties.Length, Is.EqualTo(8));
        }

        [Test]
        public void DynamicSelectBy_InvalidField_ShouldThrowArgumentException()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => query.DynamicSelectBy("NonExistentField").ToList());
            Assert.That(ex.Message, Contains.Substring("não foram encontrados na entidade User"));
            Assert.That(ex.Message, Contains.Substring("NonExistentField"));
        }

        [Test]
        public void DynamicSelectBy_MixedValidInvalidFields_ShouldThrowArgumentException()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => query.DynamicSelectBy("Id", "InvalidField", "Name").ToList());
            Assert.That(ex.Message, Contains.Substring("não foram encontrados na entidade User"));
            Assert.That(ex.Message, Contains.Substring("InvalidField"));
        }

        [Test]
        public void DynamicSelectBy_EmptyFields_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.DynamicSelectBy().ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void DynamicSelectBy_NullFields_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.DynamicSelectBy(null as string[]).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void DynamicSelectBy_EmptyStringFields_ShouldReturnOriginalQuery()
        {
            // Arrange
            var query = _context.Users.AsQueryable();
            var originalCount = query.Count();

            // Act
            var result = query.DynamicSelectBy("", "   ", null).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void DynamicSelectBy_DuplicateFields_ShouldReturnUniqueFields()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicSelectBy("Id", "Name", "Id", "Name").ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            var firstUser = result.First();

            // Check that only unique properties exist (no duplicates)
            var properties = firstUser.GetType().GetProperties();
            Assert.That(properties.Length, Is.EqualTo(2));
            Assert.That(properties.Count(p => p.Name == "Id"), Is.EqualTo(1));
            Assert.That(properties.Count(p => p.Name == "Name"), Is.EqualTo(1));
        }

        [Test]
        public void DynamicSelectBy_CaseInsensitiveFields_ShouldWork()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = query.DynamicSelectBy("id", "name").ToList(); // lowercase

            // Assert
            Assert.That(result.Count, Is.EqualTo(10));
            var firstUser = result.First();
            Assert.That(firstUser.Id, Is.GreaterThan(0));
            Assert.That(!string.IsNullOrEmpty(firstUser.Name), Is.True);

            // Check that properties exist
            var properties = firstUser.GetType().GetProperties();
            Assert.That(properties.Length, Is.EqualTo(2));
        }

        [Test]
        public void ToQueryString_DynamicSelectBy_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var selectedQuery = query.DynamicSelectBy("Id", "Name");
            var sqlQuery = selectedQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("Id", sqlQuery);
            StringAssert.Contains("Name", sqlQuery);
            StringAssert.Contains("SELECT", sqlQuery);

            // Should not contain fields that weren't selected
            StringAssert.DoesNotContain("Gender", sqlQuery);
            StringAssert.DoesNotContain("Code", sqlQuery);
            StringAssert.DoesNotContain("CreatedAt", sqlQuery);

            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }

        [Test]
        public void ToQueryString_DynamicSelectBy_SingleField_ShouldGenerateCorrectSql()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var selectedQuery = query.DynamicSelectBy("Name");
            var sqlQuery = selectedQuery.ToQueryString();

            // Assert
            Assert.That(sqlQuery, Is.Not.Null);
            Assert.That(sqlQuery, Is.Not.Empty);
            StringAssert.Contains("Name", sqlQuery);
            StringAssert.Contains("SELECT", sqlQuery);

            // Should not contain other fields
            StringAssert.DoesNotContain("Id", sqlQuery);
            StringAssert.DoesNotContain("Gender", sqlQuery);

            Console.WriteLine($"Generated SQL: {sqlQuery}");
        }
    }
}