using ECommerce_API.DataContext;
using ECommerce_API.Services.Abstract;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_API.Services.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IBasketService _basketService;
       

        public OrderService(ApplicationDbContext context, IAuthService authService,IBasketService basketService)
        {
            _context = context;
            _authService = authService;
            _basketService = basketService;
        }

        public async Task<ServiceResponse<List<Order>>> GetOrderByUser()
        {
            var user = _authService.GetUserId();
            var result = await _context.Orders.Where(x => x.UserID == user).ToListAsync();
            if (result == null) 
                {
                return new ServiceResponse<List<Order>>
                {
                    Success = false,
                    Messagge = "You dond have any order",
                };

            }
            else
            {
                return new ServiceResponse<List<Order>>
                {
                    Data= result,
                    Success = true,
                };
            }
        }

        public async Task<ServiceResponse<bool>> Refund(int orderId)
        {
            var user = _authService.GetUserId() ;
            var AnyOrder = await _context.Orders.AnyAsync(x => x.UserID == user);
            var response = await _context.Orders.FirstOrDefaultAsync(x =>x.Id == orderId);

            if(AnyOrder == true)
            {
                response.Status = false;
                _context.Orders.Update(response);
                await _context.SaveChangesAsync();
            }
            return new ServiceResponse<bool> { Success = false, };
        }

        public async Task<ServiceResponse<List<Order>>> StoreCartItem(List<Order> order)
        {
            var user = _authService.GetUserId();
            var result = await _context.Baskets.Where(x=>x.UserId == user).ToListAsync();

            var User = await _context.Users.FirstOrDefaultAsync(x => x.Id == user);

            decimal count = 0;
            if (result == null)
            {
                return new ServiceResponse<List<Order>> { Success = false, Messagge = "YOU DONT HAVE ANY BASKET ID" };
            }
            foreach (var item in result)
            {
                count = count + item.TotalPrice;
            }

            foreach (var item in result)
            {
                Order deneme = new Order();
                deneme.ProductName = item.ProductName;
                deneme.UserID = item.UserId;
                deneme.ProductId = item.ProductId;
                deneme.ProductPrice = item.Price;
                deneme.Status = true;
                deneme.TotalPrice = item.TotalPrice;
                order.Add(deneme);
                await _basketService.DeleteBasket(item);
            }
            _context.Orders.AddRange(order);
            await _context.SaveChangesAsync();
            return new ServiceResponse<List<Order>>
            {
                Success = true,
                Messagge = "Your order is success",
            };
        }
    }
}
