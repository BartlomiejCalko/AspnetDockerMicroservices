using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]

    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly DiscountGrpcService _applyDiscount;

        public BasketController(IBasketRepository basketRepository, DiscountGrpcService applyDiscount)
        {
            _basketRepository = basketRepository ?? throw new ArgumentNullException(nameof(basketRepository));
            _applyDiscount = applyDiscount ?? throw new ArgumentNullException(nameof(applyDiscount));
        }

        [HttpGet("{username}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket (string username)
        {
            var basket = await _basketRepository.GetBasket(username);   
            return Ok(basket ?? new ShoppingCart(username));  
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart shoppingCart)
        {
            // applying discount for items
            foreach (var item in shoppingCart.Items)
            {
                var coupon = await _applyDiscount.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }    
  
            return Ok(await _basketRepository.UpdateBasket(shoppingCart));
        }

        [HttpDelete("{username}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> DeleteBasket(string username)
        {
            await _basketRepository.DeleteBasket(username);
            return Ok();
        }


    }
}
