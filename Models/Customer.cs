using System;

namespace LeaderboardService.Models
{
    public class Customer
    {
        public long CustomerID { get; set; }
        public decimal Score { get; set; }
        public int Rank { get; set; }
    }
}
