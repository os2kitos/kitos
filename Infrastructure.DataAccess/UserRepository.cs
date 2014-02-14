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

        public User GetById(int id)
        {
            return _users.SingleOrDefault(u => u.Id == id);
        }

        public User GetByEmail(string email)
        {
            return _users.SingleOrDefault(u => u.Email == email);
        }

        public void Update(User user)
        {
            
        }

        private void InitFakeUsers()
        {
            _users = new List<User>()
            {
                new User()
                    {
                        Id = 0,
                        Name = "Simon Lynn-Pedersen",
                        Email = "slp@it-minds.dk",
                        Password = "slp",
                        Role = new Role() {Id = 1, Name = "Admin"}
                    },
                new User()
                    {
                        Id = 0,
                        Name = "Arne Hansen",
                        Email = "arne@it-minds.dk",
                        Password = "arne",
                        Role = new Role() {Id = 0, Name = "User"}
                    }
            };
        }
    }
}