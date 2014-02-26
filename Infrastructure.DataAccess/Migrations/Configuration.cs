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
            SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());

            //Use a smaller key size for our migration history table. See MySqlHistoryContext.cs
            SetHistoryContextFactory("MySql.Data.MySqlClient", (conn, schema) => new MySqlHistoryContext(conn, schema));
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
            var admin = new Role {Name = "Admin"};
            
            context.Roles.AddOrUpdate(x => x.Name, admin);
            
            #endregion

            #region Users

            var simon = new User
            {
                Name = "Simon Lynn-Pedersen",
                Email = "slp@it-minds.dk",
                Password = "slp",
                Role = admin
            };
            var arne = new User
            {
                Name = "Arne Hansen",
                Email = "arne@it-minds.dk",
                Password = "arne",
                Role = null
            };

            context.Users.AddOrUpdate(x => x.Email, simon, arne);

            context.SaveChanges();

            #endregion

            #region Password Reset Requests

            var simonId = context.Users.Single(x => x.Email == "slp@it-minds.dk").Id;
            var arneId = context.Users.Single(x => x.Email == "arne@it-minds.dk").Id;

            context.PasswordResetRequests.AddOrUpdate(x => x.Id,
                                                      new PasswordResetRequest
                                                      {
                                                          //This reset request is fine
                                                          Id = "workingRequest", //ofcourse, this should be a hashed string or something obscure
                                                          Time = DateTime.Now.AddYears(+20), //.MaxValue also seems to be out-of-range, but this should hopefully be good enough
                                                          User_Id = simonId
                                                      },
                                                      new PasswordResetRequest
                                                      {
                                                          //This reset request is too old
                                                          Id = "outdatedRequest",
                                                          Time = DateTime.Now.AddYears(-1), // .MinValue is out-of-range of the SQL datetime type
                                                          User_Id = arneId
                                                      }
                );

            #endregion

            base.Seed(context);
        }
    }
}
