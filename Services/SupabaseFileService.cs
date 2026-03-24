using Supabase;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

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
            // First, check if the bucket exists. If not, try to create it.
            // Note: This requires the service_role key. If using the anon key, this might fail.
            try 
            {
                var buckets = await _supabase.Storage.ListBuckets();
                if (buckets == null || !buckets.Any(b => b.Name == bucketName))
                {
                    // Attempt to create the bucket if it doesn't exist
                    await _supabase.Storage.CreateBucket(bucketName, new Supabase.Storage.BucketUpsertOptions { Public = true });
                    Console.WriteLine($"Bucket '{bucketName}' created successfully.");
                }
            }
            catch (Exception ex)
            {
                // We log but continue, as the bucket might already exist even if we can't list it (anon key permissions)
                Console.WriteLine($"Warning: Could not verify or create bucket '{bucketName}': {ex.Message}");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var fileData = stream.ToArray();

            // Match the user's RLS policy by putting files in the 'private' folder
            var fileName = $"private/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            
            try 
            {
                // Upload to Supabase Storage
                await _supabase.Storage
                    .From(bucketName)
                    .Upload(fileData, fileName);

                // Get Public URL
                return _supabase.Storage
                    .From(bucketName)
                    .GetPublicUrl(fileName);
            }
            catch (Supabase.Storage.Exceptions.SupabaseStorageException ex) when (ex.Message.Contains("Bucket not found"))
            {
                throw new Exception($"The storage bucket '{bucketName}' does not exist in your Supabase project. Please create it manually in your Supabase dashboard under Storage with 'Public' access enabled.");
            }
            catch (Supabase.Storage.Exceptions.SupabaseStorageException ex) when (ex.Message.Contains("row-level security policy"))
            {
                throw new Exception($"Access Denied: Your Supabase storage bucket '{bucketName}' has Row-Level Security (RLS) enabled but no policy allows uploads. Please add an 'INSERT' policy for 'Authenticated' or 'Anon' users in your Supabase Dashboard.");
            }
        }
    }
}
