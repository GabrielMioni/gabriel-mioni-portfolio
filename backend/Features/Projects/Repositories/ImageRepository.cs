using backend.Data;
using backend.Features.Projects.Models;

namespace backend.Features.Projects.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _context;
        public ImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Image> AddImageAsync(Image newImage)
        {
            _context.Images.Add(newImage);
            await _context.SaveChangesAsync();
            return newImage;
        }
    }
}
