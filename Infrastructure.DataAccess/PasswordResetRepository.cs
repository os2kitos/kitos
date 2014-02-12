using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Infrastructure.DataAccess
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private List<PasswordReset> _resets = new List<PasswordReset>();

        public PasswordResetRepository()
        {
            InitFakeResets();
        }

        public void Create(PasswordReset passwordReset)
        {
            //TODO
        }

        public PasswordReset Get(string hash)
        {
            return _resets.SingleOrDefault(r => r.Hash == hash);
        }

        public void Delete(PasswordReset passwordReset)
        {
            //TODO
        }

        //Fake it 'till you make it
        private void InitFakeResets()
        {
            _resets = new List<PasswordReset>
                {
                    new PasswordReset
                        {
                            //This reset request is fine
                            Id = 0,
                            Hash = "workingRequest", //ofcourse, this should be a hashed string
                            Time = DateTime.Now.AddHours(-1),
                            User = new User()
                                {
                                    Id = 0,
                                    Username = "Admin",
                                    Password = "Admin1234",
                                    Email = "admin@localhost",
                                    Roles = new List<Role>()
                                        {
                                            new Role() {Id = 0, Name = "User"},
                                            new Role() {Id = 1, Name = "Admin"}
                                        }
                                }
                        },
                    new PasswordReset
                        {
                            //This reset request is outdated
                            Id = 0,
                            Hash = "outdatedRequest",
                            Time = DateTime.Now.AddHours(-5),
                            User = new User()
                                {
                                    Id = 0,
                                    Username = "Arne",
                                    Password = "Password",
                                    Email = "arne@localhost",
                                    Roles = new List<Role>()
                                        {
                                            new Role() {Id = 0, Name = "User"},
                                        }
                                }
                        }
                };
        }
    }
}