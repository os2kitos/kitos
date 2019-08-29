﻿using System;
using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Model.Authentication
{
    public class KitosApiToken
    {
        public User User { get; }
        public string Value { get; }
        public DateTime Expiration { get; }

        public KitosApiToken(User user, string value, DateTime expiration)
        {
            User = user;
            Value = value;
            Expiration = expiration;
        }
    }
}