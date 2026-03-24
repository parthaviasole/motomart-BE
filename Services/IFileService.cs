using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public interface IFileService
    {
        Task<string> UploadFile(IFormFile file, string bucketName);
    }
}
