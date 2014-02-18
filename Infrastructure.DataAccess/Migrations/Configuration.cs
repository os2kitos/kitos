using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.DomainModel;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Infrastructure.DataAccess.KitosContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Infrastructure.DataAccess.KitosContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            #region Roles

            context.Roles.AddOrUpdate(x => x.Name,
                                      new Role {Name = "User"},
                                      new Role {Name = "Admin"}
                );

            var user = context.Roles.First(role => role.Name == "User");
            var admin = context.Roles.First(role => role.Name == "Admin");

            #endregion

            #region Users

            context.Users.AddOrUpdate(x => x.Email,
                                      new User
                                          {
                                              Name = "Simon Lynn-Pedersen",
                                              Email = "slp@it-minds.dk",
                                              Password = "slp",
                                              Roles = new Collection<Role> {admin, user}
                                          },
                                      new User
                                          {
                                              Id = 0,
                                              Name = "Arne Hansen",
                                              Email = "arne@it-minds.dk",
                                              Password = "arne",
                                              Roles = new Collection<Role> {user}
                                          }
                );

            var simon = context.Users.Single(x => x.Email == "slp@it-minds.dk");
            var arne = context.Users.Single(x => x.Email == "arne@it-minds.dk");

            #endregion

            #region Password Reset Requests
            
            context.PasswordResetRequests.AddOrUpdate(x => x.Hash,
                                                      new PasswordResetRequest
                                                          {
                                                              //This reset request is fine
                                                              Id = 0,
                                                              Hash = "workingRequest", //ofcourse, this should be a hashed string or something obscure
                                                              Time = DateTime.Now.AddHours(-3),
                                                              User = simon
                                                          },
                                                      new PasswordResetRequest
                                                          {
                                                              //This reset request is too old
                                                              Id = 0,
                                                              Hash = "outdatedRequest",
                                                              Time = DateTime.Now.AddHours(-13),
                                                              User = arne
                                                          }
                );

            #endregion
        }
    }
}
