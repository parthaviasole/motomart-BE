using Supabase;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace motomart_BE.Services
{
    public class SupabaseFileService : IFileService
    {
        private readonly Supabase.Client _supabase;

        public SupabaseFileService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task<string> UploadFile(IFormFile file, string bucketName)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileData = stream.ToArray();

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            
            // Upload to Supabase Storage
            await _supabase.Storage
                .From(bucketName)
                .Upload(fileData, fileName);

            // Get Public URL
            return _supabase.Storage
                .From(bucketName)
                .GetPublicUrl(fileName);
        }
    }
}
