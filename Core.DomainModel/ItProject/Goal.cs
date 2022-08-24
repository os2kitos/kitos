using System;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal.
    /// </summary>
    public class Goal : Entity, IProjectModule, ISupportsUserSpecificAccessControl
    {
        /// <summary>
        /// Human readable identifier.
        /// </summary>
        /// <remarks>
        /// Called "brugervendt nøgle" in OIO.
        /// </remarks>
        /// <value>
        /// The human readable identifier.
        /// </value>
        public string HumanReadableId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public bool Measurable { get; set; }
        public TrafficLight Status { get; set; }

        public int? GoalTypeId { get; set; }
        public virtual GoalType GoalType { get; set; }

        public int GoalStatusId { get; set; }
        public virtual GoalStatus GoalStatus { get; set; }

        public DateTime? SubGoalDate1 { get; set; }
        public DateTime? SubGoalDate2 { get; set; }
        public DateTime? SubGoalDate3 { get; set; }

        public string SubGoal1 { get; set; }
        public string SubGoal2 { get; set; }
        public string SubGoal3 { get; set; }

        public string SubGoalRea1 { get; set; }
        public string SubGoalRea2 { get; set; }
        public string SubGoalRea3 { get; set; }

        public bool HasUserWriteAccess(User user)
        {
            return GoalStatus != null && GoalStatus.HasUserWriteAccess(user);
        }
    }
}
