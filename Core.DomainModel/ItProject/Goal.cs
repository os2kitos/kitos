namespace Core.DomainModel.ItProject
{
    public class Goal
    {
        public int Id { get; set; }
        public int GoalStatus_Id { get; set; }

        public virtual GoalStatus GoalStatus { get; set; }
    }
}
