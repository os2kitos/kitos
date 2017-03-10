﻿using System;

namespace Core.DomainModel
{
    public interface IEntity
    {
        int Id { get; set; }
        DateTime LastChanged { get; set; }
        User LastChangedByUser { get; set; }
        int? LastChangedByUserId { get; set; }
        User ObjectOwner { get; set; }
        int? ObjectOwnerId { get; set; }

        bool HasUserWriteAccess(User user);
    }
}