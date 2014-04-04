namespace Core.DomainModel.ItProject
{
    public class Goal
    {
        public int Id { get; set; }
        public int GoalStatusId { get; set; }

        public virtual GoalStatus GoalStatus { get; set; }
    }
}
