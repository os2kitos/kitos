using System;
using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class EconomyRowDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<EconomySetDTO> Values { get; set; }
    }
}