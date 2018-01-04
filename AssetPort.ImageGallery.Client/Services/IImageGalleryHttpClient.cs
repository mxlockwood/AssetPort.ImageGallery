using System.Net.Http;
using System.Threading.Tasks;

namespace AssetPort.ImageGallery.Client.Services
{
    public interface IImageGalleryHttpClient
    {
        Task<HttpClient> GetClient();
    }
}
