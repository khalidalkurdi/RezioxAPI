using Reziox.Model.ThePlace;

namespace Rezioxgithub.DataAccess.Repository
{
    public interface IPlaceRepository : IRepository<Place>
    {
        void Update(Place entity);
    }
}
