﻿namespace Core.DomainModel.Events
{
    public interface IDomainEvents
    {
        /// <summary>
        /// Raises the given domain event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        void Raise<T>(T args) where T : IDomainEvent;
    }
}