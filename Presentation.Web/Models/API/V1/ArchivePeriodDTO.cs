﻿using System;

namespace Presentation.Web.Models.API.V1
{
    public class ArchivePeriodDTO
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string UniqueArchiveId { get; set; }
        public int ItSystem_Id { get; set; }
    }
}