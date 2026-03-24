using Microsoft.EntityFrameworkCore;
using motomart_BE.Data;
using motomart_BE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace motomart_BE.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<Address>> GetAddressesByUserId(Guid userId);
        Task<Address?> GetAddressById(Guid id);
        Task<Address> CreateAddress(Address address);
        Task<Address> UpdateAddress(Address address);
        Task<bool> DeleteAddress(Guid id);
        Task<bool> SetDefaultAddress(Guid userId, Guid addressId);
    }

    public class AddressService : IAddressService
    {
        private readonly AppDbContext _context;

        public AddressService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetAddressesByUserId(Guid userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<Address?> GetAddressById(Guid id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task<Address> CreateAddress(Address address)
        {
            if (address.IsDefault)
            {
                // Reset other addresses' IsDefault to false
                var otherAddresses = await _context.Addresses
                    .Where(a => a.UserId == address.UserId)
                    .ToListAsync();
                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                }
            }
            else
            {
                // If this is the first address, make it default
                var count = await _context.Addresses.CountAsync(a => a.UserId == address.UserId);
                if (count == 0)
                {
                    address.IsDefault = true;
                }
            }

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<Address> UpdateAddress(Address address)
        {
            if (address.IsDefault)
            {
                var otherAddresses = await _context.Addresses
                    .Where(a => a.UserId == address.UserId && a.Id != address.Id)
                    .ToListAsync();
                foreach (var addr in otherAddresses)
                {
                    addr.IsDefault = false;
                }
            }

            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<bool> DeleteAddress(Guid id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null) return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            // If we deleted the default address, make another one default if available
            if (address.IsDefault)
            {
                var nextAddress = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == address.UserId);
                if (nextAddress != null)
                {
                    nextAddress.IsDefault = true;
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> SetDefaultAddress(Guid userId, Guid addressId)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var targetAddress = addresses.FirstOrDefault(a => a.Id == addressId);
            if (targetAddress == null) return false;

            foreach (var addr in addresses)
            {
                addr.IsDefault = (addr.Id == addressId);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
