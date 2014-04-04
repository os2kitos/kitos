﻿using System.Data.Entity.ModelConfiguration;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;

namespace Infrastructure.DataAccess.Mapping
{
    public class ItSystemRightMap : EntityTypeConfiguration<ItSystemRight>
    {
        public ItSystemRightMap()
        {
            this.HasKey(right => new { right.ObjectId, right.RoleId, right.UserId });

            this.ToTable("ItSystemRight");

            this.HasRequired(right => right.Object)
                .WithMany(proj => proj.Rights)
                .HasForeignKey(right => right.ObjectId);

            this.HasRequired(right => right.Role)
                .WithMany(role => role.References)
                .HasForeignKey(right => right.RoleId);

            this.HasRequired(right => right.User)
                .WithMany(user => user.SystemRights)
                .HasForeignKey(right => right.UserId);
        }
    }
}