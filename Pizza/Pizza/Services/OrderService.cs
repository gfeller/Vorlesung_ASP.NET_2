using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Pizza.Data;
using Pizza.Models;
using Pizza.Utilities;
using Pizza.ViewModels;
using Pizza_Demo.Exceptions;

namespace Pizza.Services
{
    public class OrderService {
        private readonly ApplicationDbContext _context;


        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Order> GetAll(ClaimsPrincipal user)
        {
            IEnumerable<Order> orders = _context.Order;
            if (!user.IsAdmin())
            {
                orders = orders.Where(x => x.CustomerId == user.GetId());
            }
            return orders;
        }

        public Order Add(string userId, NewOrderViewModel order)
        {
            var newOrder = new Order() {Name = order.Name, CustomerId = userId};
            _context.Order.Add(newOrder);
            _context.SaveChanges();
            return newOrder;
        }
        
        public Order Delete(ClaimsPrincipal user, long id)
        {
            var order = this.Get(user, id);
            user.EnsurePermission(order.CustomerId);

            if (order.State == OrderState.New)
            {
                order.State = OrderState.Deleted;
                _context.SaveChanges();
                return order;
            }
            else
            {
                throw new ServiceException(ServiceExceptionType.ForbiddenByRule);
            }
        } 

        public Order Get(ClaimsPrincipal user, long id)
        {
            var order = _context.Order.Where(x => x.Id == id).Include(x => x.Customer).FirstOrDefault();
            if (order == null)
            {
                throw new ServiceException(ServiceExceptionType.NotFound);
            }

            user.EnsurePermission(order.CustomerId);
            return order;
        }
    }
}
