using Dapper;
using Discount.API.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _configuration;

        public DiscountRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
            using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>
                ("select * from coupon where ProductName = @productName", new { ProductName = productName });

            if (coupon == null)
                return new Coupon
                { ProductName = "No discount", Amount = 0, Description = "No discount description" };
            return coupon; 
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affectedDiscount = await connection.ExecuteAsync
                ("insert into coupon (ProductName, Description, Amount) values (@ProductName, @Description, @Amount)"
                , new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount });

            if (affectedDiscount == 0)
                return false;   

            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affectedDiscount = await connection.ExecuteAsync
                ("update coupon set ProductName = @ProductName, Description = @Description, Amount = @Amount where Id = @Id)"
                , new { ProductName = coupon.ProductName, Description = coupon.Description, Amount = coupon.Amount, Id = coupon.Id });

            if (affectedDiscount == 0)
                return false;

            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            using var connection = new NpgsqlConnection
                (_configuration.GetValue<string>("DatabaseSettings:ConnectionString"));

            var affectedDiscount = await connection.ExecuteAsync
                ("delete from coupon where ProductName = @ProductName", new { ProductName = productName });

            if (affectedDiscount == 0)
                return false;

            return true;
        }
    }
}
