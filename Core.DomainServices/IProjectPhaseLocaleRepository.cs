using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainModel.ItProject;

namespace Core.DomainServices
{
    public interface IProjectPhaseLocaleRepository
    {
        ProjectPhaseLocale GetById(int mId, int pId);
        ProjectPhaseLocale Insert(ProjectPhaseLocale entity);
        void DeleteById(int mId, int pId);
        void Update(ProjectPhaseLocale entity);
        void Save(); 
    }
}