﻿namespace Core.DomainModel
{
    public interface ILocaleEntity<TOriginal>
    {
        int Municipality_Id { get; set; }
        int Original_Id { get; set; }
        string Value { get; set; }

        Municipality Municipality { get; set; }
        TOriginal Original { get; set; }
    }
}