using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.DomainModel.ItProject;

namespace Core.DomainServices
{
    public interface IProjectPhaseLocaleRepository
    {
        ProjPhaseLocale GetById(int mId, int pId);
        ProjPhaseLocale Insert(ProjPhaseLocale entity);
        void DeleteById(int mId, int pId);
        void Update(ProjPhaseLocale entity);
        void Save(); 
    }
}