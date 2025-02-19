using Leaderboard.Models;
using LeaderboardService.Models;
using System.Collections.Generic;
using System.Linq;

namespace LeaderboardService.Services
{
    public class LeaderboardService
    {
        private LeaderboardHashDoubleLinkList _linkList;

        public LeaderboardService()
        {
            this._linkList = new LeaderboardHashDoubleLinkList();

            //添加测试数据

            this._linkList.UpdateScore(53274324, 95);
            this._linkList.UpdateScore(6144320, 93);

            this._linkList.UpdateScore(15514665, 124);

            this._linkList.UpdateScore(76786448, 100);
            this._linkList.UpdateScore(254814111, 96);

            this._linkList.UpdateScore(8009471, 93);
            this._linkList.UpdateScore(38819, 92);

            this._linkList.UpdateScore(81546541, 113);
            this._linkList.UpdateScore(1745431, 100);

            this._linkList.UpdateScore(11028481, 93);
        }

        public decimal UpdateScore(long customerId, decimal scoreChange)
        {
            return this._linkList.UpdateScore(customerId, scoreChange);
        }

        public List<Customer> GetCustomersByRank(int start, int end)
        {
            return this._linkList.GetCustomersByRank(start, end);

        }

        public List<Customer> GetCustomerWithNeighbors(long customerId, int high, int low)
        {
            return this._linkList.GetCustomerWithNeighbors(customerId, high, low);
        }
    }
}