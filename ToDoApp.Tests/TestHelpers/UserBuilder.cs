using ToDoApp.Domain.Entities.User;

namespace ToDoApp.Tests.TestHelpers
{
    public class UserBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _email = "test@test.com";
        private string _firstName = "Test";
        private string _lastName = "User";

        public static UserBuilder Create() => new();

        public UserBuilder WithId(Guid id) { _id = id; return this; }
        public UserBuilder WithEmail(string email) { _email = email; return this; }
        public UserBuilder WithFirstName(string firstName) { _firstName = firstName; return this; }
        public UserBuilder WithLastName(string lastName) { _lastName = lastName; return this; }

        public User Build() => new()
        {
            Id = _id,
            Email = _email,
            FirstName = _firstName,
            LastName = _lastName,
        };
    }
}
