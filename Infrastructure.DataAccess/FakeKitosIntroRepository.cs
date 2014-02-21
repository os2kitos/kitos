using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.Text;
using Core.DomainServices;

namespace Infrastructure.DataAccess
{
    public class FakeKitosIntroRepository : IGenericRepository<KitosIntro>
    {
        private readonly IEnumerable<KitosIntro> _content;

        public FakeKitosIntroRepository()
        {
            _content = new List<KitosIntro>()
                {
                    new KitosIntro()
                        {
                            Id = 0,
                            Text = "Hej med dig!"
                        }
                };
        }

        System.Collections.Generic.IEnumerable<KitosIntro> IGenericRepository<KitosIntro>.Get(System.Linq.Expressions.Expression<System.Func<KitosIntro, bool>> filter, System.Func<System.Linq.IQueryable<KitosIntro>, System.Linq.IOrderedQueryable<KitosIntro>> orderBy, string includeProperties)
        {
            return _content;
        }

        KitosIntro IGenericRepository<KitosIntro>.GetById(int id)
        {
            return _content.SingleOrDefault(t => t.Id == id);
        }

        void IGenericRepository<KitosIntro>.Insert(KitosIntro entity)
        {
            throw new System.NotImplementedException();
        }

        void IGenericRepository<KitosIntro>.DeleteById(int id)
        {
            throw new System.NotImplementedException();
        }

        void IGenericRepository<KitosIntro>.Update(KitosIntro entity)
        {
            
        }

        void IGenericRepository<KitosIntro>.Save()
        {
            
        }

        void System.IDisposable.Dispose()
        {
            
        }
    }
}