using System.Linq;
using System.Collections.Generic;
using Core.DomainServices;
using Core.DomainModel;

namespace Infrastructure.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private List<User> _users = new List<User>();

        public UserRepository()
        {
            InitFakeUsers();
        }

        public User Get(int id)
        {
            return _users.SingleOrDefault(u => u.Id == id);
        }

        public User Get(string username)
        {
            return _users.SingleOrDefault(u => u.Username == username);
        }

        public User Get(string username, string password)
        {
            //TODO: HASHING OF PASSWORDS!!!
            return _users.SingleOrDefault(u => u.Username == username && u.Password == password);
        }

        public bool Validate(string username, string password)
        {
            return Get(username, password) != null;
        }   

        private void InitFakeUsers()
        {
            _users = new List<User>(){
                new User()
                    {
                        Id = 0,
                        Username = "Admin",
                        Password = "Admin1234",
                        Roles = new List<Role>()
                            {
                                new Role() {Id = 0, Name = "User"},
                                new Role() {Id = 1, Name = "Admin"}
                            }
                    },
                new User()
                    {
                        Id = 0,
                        Username = "User",
                        Password = "Password",
                        Roles = new List<Role>()
                            {
                                new Role() {Id = 0, Name = "User"},
                            }
                    }
            };
        }
    }
}