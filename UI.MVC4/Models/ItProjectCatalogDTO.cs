using Core.DomainModel;

namespace UI.MVC4.Models
{
    public class ItProjectCatalogDTO
    {
        //1st column: type
        public string ItProjectTypeName { get; set; }

        //2nd column: project ID
        public string ItProjectId { get; set; }

        //3rd column: parent ID
        public string ParentItProjectId { get; set; }

        //4th column: name + link
        public int Id { get; set; }
        public string Name { get; set; }

        //5th column: category
        public string ItProjectCategoryName { get; set; }
        
        //6th and 7th column: public, archived
        public AccessModifier AccessModifier { get; set; }
        public bool IsArchived { get; set; }

        //8th column: responsible org unit
        public string ResponsibleOrgUnitName { get; set; }

        //9th column object owner
        public string ObjectOwnerName { get; set; }
    }
}