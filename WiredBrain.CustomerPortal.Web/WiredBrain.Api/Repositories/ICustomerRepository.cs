using System.Threading.Tasks;
using WiredBrain.Api.Data;
using WiredBrain.Api.Models;

namespace WiredBrain.Api.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetCustomerByLoyaltyNumber(int loyaltyNumber);
        Task SetFavorite(UpdateFavoriteModel model);
    }
}