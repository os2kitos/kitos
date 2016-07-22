using System;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal.
    /// </summary>
    public class Goal : Entity, IContextAware, IProjectModule
    {
        /// <summary>
        /// Human readable identifier.
        /// </summary>
        /// <remarks>
        /// Called "brugervendt n�gle" in OIO.
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

        /// <summary>
        /// Determines whether a user has write access to this instance.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>
        ///   <c>true</c> if user has write access, otherwise <c>false</c>.
        /// </returns>
        public override bool HasUserWriteAccess(User user)
        {
            if (GoalStatus != null && GoalStatus.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }

        /// <summary>
        /// Determines whether this instance is within a given organizational context.
        /// </summary>
        /// <param name="organizationId">The organization identifier (context) the user is accessing from.</param>
        /// <returns>
        ///   <c>true</c> if this instance is in the organizational context, otherwise <c>false</c>.
        /// </returns>
        public bool IsInContext(int organizationId)
        {
            if (GoalStatus != null)
                return GoalStatus.IsInContext(organizationId);

            return false;
        }
    }
}
