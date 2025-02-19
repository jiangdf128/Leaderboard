using System;
using LeaderboardService.Models;

namespace Leaderboard.Models;

public class CustomerNode
{
    public Customer Customer {get;set;}

    public CustomerNode? Previous {get;set;}

    public CustomerNode? Next {get;set;}

    public CustomerNode(long customerId,decimal score){
        this.Customer=new Customer { CustomerID = customerId, Score = score, Rank = 0 };
    }
}
