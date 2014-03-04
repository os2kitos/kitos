﻿namespace UI.MVC4.Models
{
    public abstract class DropDownDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }  
    }
}