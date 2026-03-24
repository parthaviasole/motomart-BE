using motomart_BE.Data;
using motomart_BE.Helpers;
using motomart_BE.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public interface IUserService
    {
        Task<PagedList<User>> GetUsers(int pageNumber, int pageSize, string? searchTerm = null);
        Task<User?> GetUserById(Guid id);
        Task<User?> GetUserByEmail(string email);
        Task<User> UpdateUser(User user);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<User>> GetUsers(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm));
            }

            return await PagedList<User>.CreateAsync(query.OrderByDescending(u => u.CreatedAt), pageNumber, pageSize);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> UpdateUser(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
