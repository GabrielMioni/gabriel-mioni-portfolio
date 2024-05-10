using backend.Data;
using backend.Features.Projects.Models;

namespace backend.Features.Projects.Repositories
{
    public interface IImageRepository
    {
        Task<Image> AddImageAsync(Image newImage);
    }
}
