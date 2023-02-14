﻿using System;

namespace Core.ApplicationServices.Model.Shared.Write
{
    public class UpdatedExternalReferenceProperties
    {
        public Guid? Uuid { get; set; }
        public string Title { get; set; }
        public string DocumentId { get; set; }
        public string Url { get; set; }
        public bool MasterReference { get; set; }
    }
}
