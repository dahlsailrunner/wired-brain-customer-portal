using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WiredBrain.CustomerPortal.Web.Data;
using WiredBrain.CustomerPortal.Web.Models;

namespace WiredBrain.CustomerPortal.Web.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerPortalDbContext dbContext;

        public CustomerRepository(CustomerPortalDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Customer> GetCustomerByLoyaltyNumber(int loyaltyNumber)
        {
            var customers = dbContext.Customers.FromSqlRaw("SELECT * FROM Customers where LoyaltyNumber = " + loyaltyNumber);
            return await customers.FirstOrDefaultAsync();
        }

        public async Task SetFavorite(EditFavoriteModel model)
        {
            var customers = dbContext.Customers.FromSqlRaw("SELECT * FROM Customers where LoyaltyNumber = " + model.LoyaltyNumber.ToString());
            var customer = await customers.FirstOrDefaultAsync();

            customer.FavoriteDrink = model.Favorite;
            await dbContext.SaveChangesAsync();
        }
    }
}
