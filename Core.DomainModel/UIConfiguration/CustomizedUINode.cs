namespace Core.DomainModel.UIConfiguration
{
    public class CustomizedUINode : Entity
    {
        public int ModuleId{ get; set; }
        
        /// <summary>
        /// Contains a route to the property, for which visibility is being set
        /// e.g. module.group.field
        /// </summary>
        public string Key { get; set; }
        public bool Enabled { get; set; }

        public virtual UIModuleCustomization UiModuleCustomization { get; set; }
    }
}
