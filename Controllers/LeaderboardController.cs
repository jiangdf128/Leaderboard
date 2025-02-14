using LeaderboardService.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardService.Controllers
{
    [ApiController]
    // [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly LeaderboardService.Services.LeaderboardService _leaderboardService;

        public LeaderboardController(LeaderboardService.Services.LeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        // POST /customer/{customerid}/score/{score}
        [HttpPost("customer/{customerid}/score/{score}")]
        public ActionResult<decimal> UpdateScore(long customerId, decimal score)
        {
            var updatedScore = _leaderboardService.UpdateScore(customerId, score);
            return Ok(updatedScore);
        }

        // GET /leaderboard?start={start}&end={end}
        [HttpGet("leaderboard")]
        public ActionResult<IQueryable<Customer>> GetCustomersByRank([FromQuery] int start, [FromQuery] int end)
        {
            var customers = _leaderboardService.GetCustomersByRank(start, end);
            return Ok(customers);
        }

        // GET /leaderboard/{customerid}?high={high}&low={low}
        [HttpGet("leaderboard/{customerid}")]
        public ActionResult<IQueryable<Customer>> GetCustomerWithNeighbors(long customerid, [FromQuery] int high = 0, [FromQuery] int low = 0)
        {
            var customers = _leaderboardService.GetCustomerWithNeighbors(customerid, high, low);
            return Ok(customers);
        }
    }
}