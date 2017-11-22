namespace Core.DomainModel.ItContract
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class ItContractAgreementElementTypes
    {
        public int ItContract_Id { get; set; }
        public virtual ItContract ItContract { get; set; }

        public int AgreementElementType_Id { get; set; }
        public virtual AgreementElementType AgreementElementType { get; set;}
    }
}
