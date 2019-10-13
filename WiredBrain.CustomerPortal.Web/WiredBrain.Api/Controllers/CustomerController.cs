using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WiredBrain.Api.Data;
using WiredBrain.Api.Repositories;

namespace WiredBrain.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _repo;

        public CustomerController(ICustomerRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<Customer> Get(int loyaltyNumber)
        {
            return await _repo.GetCustomerByLoyaltyNumber(loyaltyNumber);
        }
    }
}