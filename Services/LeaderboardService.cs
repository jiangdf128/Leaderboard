
using LeaderboardService.Models;
namespace LeaderboardService.Services
{
    public class LeaderboardService
    {
        private List<Customer> _leaderboardList;

        private readonly Lock _lock = new();

        public LeaderboardService()
        {
            _leaderboardList = [];
        }

        public decimal UpdateScore(long customerId, decimal scoreChange)
        {
            lock (_lock)
            {
                var customer = _leaderboardList.FirstOrDefault(c => c.CustomerID == customerId);

                if (customer == null)
                {
                    // Add new customer if not found
                    customer = new Customer { CustomerID = customerId, Score = scoreChange, Rank = 0 };
                    _leaderboardList.Add(customer);
                }
                else
                {
                    // Update existing customer's score
                    customer.Score += scoreChange;
                }

                // Recalculate ranks after score change
                _leaderboardList = _leaderboardList.OrderByDescending(c => c.Score).ThenBy(c => c.CustomerID).ToList();
                for (int i = 0; i < _leaderboardList.Count; i++)
                {
                    _leaderboardList[i].Rank = i + 1;
                }

                return customer.Score;
            }
        }

        public List<Customer> GetCustomersByRank(int start, int end)
        {
            lock (_lock)
            {
                return _leaderboardList.Where(c => c.Rank >= start && c.Rank <= end).ToList();
            }
        }

        public List<Customer> GetCustomerWithNeighbors(long customerId, int high, int low)
        {
            lock (_lock)
            {
                var customer = _leaderboardList.FirstOrDefault(c => c.CustomerID == customerId);

                if (customer == null)
                {
                    return new List<Customer>(); // Customer not found
                }

                var rank = customer.Rank;

                var neighbors = _leaderboardList
                    .Where(c => c.Rank >= rank - high && c.Rank <= rank + low)
                    .OrderBy(c => c.Rank)
                    .ToList();

                return neighbors;
            }
        }
    }

}
