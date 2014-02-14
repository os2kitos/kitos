using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices;

namespace Infrastructure.DataAccess
{
    public class PasswordResetRequestRepository : IPasswordResetRequestRepository
    {
        private List<PasswordResetRequest> _resets = new List<PasswordResetRequest>();

        public PasswordResetRequestRepository()
        {
            InitFakeResets();
        }

        public void Create(PasswordResetRequest passwordReset)
        {
            //TODO
        }

        public PasswordResetRequest GetByHash(string hash)
        {
            return _resets.SingleOrDefault(r => r.Hash == hash);
        }

        public void Delete(PasswordResetRequest passwordReset)
        {
            //TODO
        }

        //Fake it 'till you make it
        private void InitFakeResets()
        {
            _resets = new List<PasswordResetRequest>
                {
                    new PasswordResetRequest
                        {
                            //This reset request is fine
                            Id = 0,
                            Hash = "workingRequest", //ofcourse, this should be a hashed string or something obscure
                            Time = DateTime.Now.AddHours(-3),
                            User = new User()
                                {
                                    Id = 0,
                                    Name = "Simon Lynn-Pedersen",
                                    Email = "slp@it-minds.dk",
                                    Password = "slp",
                                    Role = new Role() {Id = 1, Name = "Admin"}
                                }
                        },
                    new PasswordResetRequest
                        {
                            //This reset request is too old
                            Id = 0,
                            Hash = "outdatedRequest",
                            Time = DateTime.Now.AddHours(-13),
                            User = new User()
                                {
                                    Id = 0,
                                    Name = "Arne Hansen",
                                    Email = "arne@it-minds.dk",
                                    Password = "arne",
                                    Role = new Role() {Id = 0, Name = "User"}
                                }
                        }
                };
        }
    }
}