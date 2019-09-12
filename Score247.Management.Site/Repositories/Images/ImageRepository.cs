using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.Images;

namespace Score247.Management.Site.Repositories.Images
{
    public class ImageRepository
    {
        private readonly IDynamicRepository repository;

        public ImageRepository()
        {
            repository = new DynamicRepository();
        }

        public async Task<int> InsertImage(InsertImageCriteria criteria)
        {
            return await repository.ExecuteAsync(criteria);
        }
    }
}