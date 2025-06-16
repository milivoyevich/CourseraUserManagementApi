using System.Collections.Generic;
using CourseraUserManagementApi.Models;
public static class MockData
{
    public static List<User> Users = new List<User>
    {
        new User { Id = 1, Username = "Alice", Email = "alice@example.com" },
        new User { Id = 2, Username = "Bob", Email = "bob@example.com" },
        new User { Id = 3, Username = "Charlie", Email = "charlie@example.com" }
    };
}