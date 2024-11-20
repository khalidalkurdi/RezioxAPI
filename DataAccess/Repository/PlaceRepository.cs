using Reziox.DataAccess;
using Reziox.Model.ThePlace;
using System.Linq.Expressions;

namespace Rezioxgithub.DataAccess.Repository
{
    public class PlaceRepository :Repository<Place> ,IPlaceRepository
    {
        private readonly AppDbContext _db;
        public PlaceRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Place entity)
        {
            _db.Places.Update(entity);
        }
    }
}
