using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiredBrain.Api.Data;
using WiredBrain.Api.Models;

namespace WiredBrain.Api.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerPortalDbContext _dbContext;

        public CustomerRepository(CustomerPortalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Customer> GetCustomerByLoyaltyNumber(int loyaltyNumber)
        {
            var customers = _dbContext.Customers.FromSqlRaw("SELECT * FROM Customers where LoyaltyNumber = " + loyaltyNumber);
            var customer = await customers.FirstOrDefaultAsync();

            return customer;
        }

        public async Task SetFavorite(UpdateFavoriteModel model)
        {
            var customers = _dbContext.Customers.FromSqlRaw("SELECT * FROM Customers where LoyaltyNumber = " + model.LoyaltyNumber);
            var customer = await customers.FirstOrDefaultAsync();

            customer.FavoriteDrink = model.Favorite;
            await _dbContext.SaveChangesAsync();
        }
    }
}
