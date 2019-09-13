using System.Linq;
using System.Threading.Tasks;
using Fanex.Data.Repository;
using Score247.Management.Site.Criterias.Images;
using Score247Management.Core.Images;

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

        public async Task<ImageSource> GetCountryImage(string shortName)
        {
            var criteria = new GetCountryFlagCriteria() { ShortName = shortName };
            var images = await repository.FetchAsync<ImageSource>(criteria);

            return images.OrderBy(x => x.IsOriginal).FirstOrDefault();
        }
    }
}