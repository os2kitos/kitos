using System;
using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItProject;

namespace Core.DomainServices.Factories
{
    public class ItProjectFactory
    {
        public static ItProject Create(string name, int organizationId, DateTime now)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var itProject = new ItProject
            {
                Name = name,
                OrganizationId = organizationId,
            };

            InitializeAccessModifier(itProject);

            InitializePhases(itProject);

            InitializeEconomyYears(itProject);

            InitializeHandover(itProject);

            InitializeGoalStatus(itProject);

            return itProject;
        }

        private static void InitializeGoalStatus(ItProject itProject)
        {
            itProject.GoalStatus = new GoalStatus();
        }

        private static void InitializeHandover(ItProject itProject)
        {
            itProject.Handover = new Handover();
        }

        private static void InitializePhases(ItProject itProject)
        {
            itProject.CurrentPhase = 1;
            itProject.Phase1 = CreateItProjectPhase(PhaseNames.Phase1);
            itProject.Phase2 = CreateItProjectPhase(PhaseNames.Phase2);
            itProject.Phase3 = CreateItProjectPhase(PhaseNames.Phase3);
            itProject.Phase4 = CreateItProjectPhase(PhaseNames.Phase4);
            itProject.Phase5 = CreateItProjectPhase(PhaseNames.Phase5);
        }

        private static ItProjectPhase CreateItProjectPhase(string name)
        {
            return new ItProjectPhase {Name = name};
        }

        private static void InitializeAccessModifier(ItProject itProject)
        {
            itProject.AccessModifier = AccessModifier.Local;
        }

        private static void InitializeEconomyYears(ItProject project)
        {
            project.EconomyYears = new List<EconomyYear>
            {
                CreateEconomyYear(project, 0),
                CreateEconomyYear(project, 1),
                CreateEconomyYear(project, 2),
                CreateEconomyYear(project, 3),
                CreateEconomyYear(project, 4),
                CreateEconomyYear(project, 5)
            };
        }

        private static EconomyYear CreateEconomyYear(ItProject project, int yearNumber)
        {
            return new EconomyYear
            {
                YearNumber = yearNumber,
            };
        }
    }
}
