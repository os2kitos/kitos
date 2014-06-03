namespace Core.DomainModel.ItProject
{
    public class Risk : Entity
    {
        public int ItProjectId { get; set; }
        public virtual ItProject ItProject { get; set; }

        public string Name { get; set; }
        public string Action { get; set; }

        public int Probability { get; set; }
        public int Consequence { get; set; }

        public int? ResponsibleUserId { get; set; }
        public virtual User ResponsibleUser { get; set; }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItProject != null && ItProject.HasUserWriteAccess(user)) return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
