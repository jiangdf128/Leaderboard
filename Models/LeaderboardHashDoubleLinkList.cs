using System;
using LeaderboardService.Models;

namespace Leaderboard.Models;

//排行榜hash双向链表。
public class LeaderboardHashDoubleLinkList
{
    //锁对象，以确保线程安全。
    private readonly Lock _lock = new();

    //Hash表，根据客户ID快速Hash定位链表节点。
    private readonly Dictionary<long, CustomerNode> _customerLookup;

    //双向链表的头和尾指针。
    private CustomerNode? head, tail;

    // Decimal? maxScore, minScore;

    public LeaderboardHashDoubleLinkList()
    {
        head = tail = null;
        // maxScore = minScore = null;
        _customerLookup = [];
    }

    public decimal UpdateScore(long customerId, decimal scoreDelta)
    {
        lock (_lock)
        {

            bool exists = this._customerLookup.ContainsKey(customerId);
            CustomerNode updateNode;
            CustomerNode? rankStartNode;
            if (exists)
            {
                updateNode = this._customerLookup[customerId];
                updateNode.Customer.Score += scoreDelta;
                if (scoreDelta == 0 ||
                ((updateNode.Previous == null || updateNode.Previous.Customer.Score > updateNode.Customer.Score || (updateNode.Previous.Customer.Score == updateNode.Customer.Score && updateNode.Previous.Customer.CustomerID < updateNode.Customer.CustomerID)) &&
                (updateNode.Next == null || updateNode.Next.Customer.Score < updateNode.Customer.Score || (updateNode.Next.Customer.Score == updateNode.Customer.Score && updateNode.Next.Customer.CustomerID > updateNode.Customer.CustomerID))))
                {
                    //如果分数变化为0，或者值变化后，依然小于前节点分数（如果存在或前节点为空）同时大于后节点分数（如果存在或后节点为空），则什么都不用操作，直接返回值。
                    //或者出现和前节点相等分数，但ID号比前节点ID号大，或者出现和后节点相等分数，但ID号比后节点ID号小。
                    return updateNode.Customer.Score;
                }

                //程序执行到这里，则节点需要移动，寻找新的位置插入。
                //1.如果朝前移动，则需要从新插入位置之后重新排名。
                //2.如果朝后移动，则需要从该节点未移动前的后节点重新排名。

                if (scoreDelta > 0)
                {
                    //1.朝前移动
                    rankStartNode = updateNode;
                }
                else
                {
                    //2.向后移动
                    rankStartNode = updateNode.Next;
                }
                this.UnlinkFromDoubleLinkList(updateNode);
                this.InsertFromStartToNext(updateNode, head);
            }
            else
            {
                updateNode = new CustomerNode(customerId, scoreDelta);
                rankStartNode = updateNode;
                //说么是新节点，从头开始遍历，寻找插入位置；
                if (head == null)
                {
                    head = tail = updateNode;
                }
                else
                {
                    this.InsertFromStartToNext(updateNode, head);
                }
                this._customerLookup.Add(customerId, updateNode);
            }
            RebuildRank(rankStartNode);

            return updateNode.Customer.Score;
        }

    }

    private void UnlinkFromDoubleLinkList(CustomerNode unlinkNode)
    {
        if (unlinkNode.Previous != null && unlinkNode.Next != null)
        {
            unlinkNode.Previous.Next = unlinkNode.Next;
            unlinkNode.Next.Previous = unlinkNode.Previous;
        }
        else if (unlinkNode.Previous != null && unlinkNode.Next == null)
        {
            this.tail = unlinkNode.Previous;
            unlinkNode.Previous.Next = null;
        }
        else if (unlinkNode.Previous == null && unlinkNode.Next != null)
        {
            this.head = unlinkNode.Next;
            unlinkNode.Next.Previous = null;
        }
        else
        {
            this.head = this.tail = null;
        }

    }

    private static void RebuildRank(CustomerNode? rankStartNode)
    {
        if (rankStartNode != null)
        {
            int startRank = rankStartNode.Previous == null ? 0 : rankStartNode.Previous.Customer.Rank;
            CustomerNode? p = rankStartNode;
            while (p != null)
            {
                startRank++;
                p.Customer.Rank = startRank;
                p = p.Next;
            }
        }
    }

    private void InsertFromStartToNext(CustomerNode insertNode, CustomerNode? startNode)
    {
        CustomerNode? currentNode = startNode;
        while (currentNode != null)
        {
            if (insertNode.Customer.Score > currentNode.Customer.Score ||
                (insertNode.Customer.Score == currentNode.Customer.Score && insertNode.Customer.CustomerID < currentNode.Customer.CustomerID))
            {
                //如果积分比当前节点大，或者积分与当前节点一样，但编号小于当前节点，则在当前节点前面插入。
                if (currentNode.Previous == null)
                {
                    this.head = insertNode;
                }
                insertNode.Previous = currentNode.Previous;
                insertNode.Next = currentNode;
                currentNode.Previous = insertNode;
                return;
            }
            if ((insertNode.Customer.Score == currentNode.Customer.Score && insertNode.Customer.CustomerID > currentNode.Customer.CustomerID && currentNode.Next != null
               && ((currentNode.Next.Customer.Score == insertNode.Customer.Score && currentNode.Next.Customer.CustomerID > insertNode.Customer.CustomerID) || currentNode.Next.Customer.Score < insertNode.Customer.Score)
               ) || (insertNode.Customer.Score < currentNode.Customer.Score && currentNode.Next != null && insertNode.Customer.Score > currentNode.Next.Customer.Score)
               || (insertNode.Customer.Score < currentNode.Customer.Score && currentNode.Next != null && insertNode.Customer.Score == currentNode.Next.Customer.Score && insertNode.Customer.CustomerID<currentNode.Next.Customer.CustomerID)
               )

            {
                //如果积分和当前节点一样，但编号大于当前节点，且与当前节点大下一节点比较，分数大于下一节点分数或者分数一致且编号小于当前节点的下一节点的情况，则插入当前节点后面。
                //或者，积分小于当前节点，且积分大于当前节点的下一节点的积分，也插入当前节点后面。
                //或者，积分小于当前节点，且积分等于当前节点的下一节点的积分，且编号小于当前节点的下一节点的编号，也插入当前节点后面。
                insertNode.Next = currentNode.Next;
                currentNode.Next.Previous = insertNode;

                insertNode.Previous = currentNode;
                currentNode.Next = insertNode;
                return;

            }
            if (currentNode.Next == null)
            {
                this.tail = insertNode;
                currentNode.Next = insertNode;
                insertNode.Previous = currentNode;
                insertNode.Next = null;
                return;
            }
            currentNode = currentNode.Next;
        }
    }

    public List<Customer> GetCustomers()
    {
        var customers = new List<Customer>();
        CustomerNode? p = head;
        while (p != null)
        {
            customers.Add(p.Customer);
            p = p.Next;
        }
        return customers;
    }

    public List<Customer> GetCustomersByRank(int start, int end)
    {
        lock (_lock)
        {
            int mid = this._customerLookup.Count / 2;
            var customers = new List<Customer>();
            if (start > end || start > this._customerLookup.Count || (start==0 && end==0))
            {
                return this.GetCustomers();
            }
            if (start <= 0)
            {
                start = 1;
            }
            if (end > this._customerLookup.Count)
            {
                end = this._customerLookup.Count;
            }
            CustomerNode? p;
            if ((start + end) / 2 <= mid)
            {
                //从头开始遍历；
                p = head;
                while (p != null)
                {
                    if (p.Customer.Rank >= start && p.Customer.Rank <= end)
                    {
                        customers.Add(p.Customer);
                    }
                    if (p.Customer.Rank == end)
                    {
                        break;
                    }
                    p = p.Next;
                }

            }
            else
            {
                //从尾开始遍历。
                p = tail;
                while (p != null)
                {
                    if (p.Customer.Rank >= start && p.Customer.Rank <= end)
                    {
                        customers.Insert(0, p.Customer);
                    }
                    if (p.Customer.Rank == start)
                    {
                        break;
                    }
                    p = p.Previous;
                }
            }

            return customers;
        }
    }

    public List<Customer> GetCustomerWithNeighbors(long customerId, int high, int low)
    {
        lock (_lock)
        {
            var customers = new List<Customer>();
            if (this._customerLookup.ContainsKey(customerId))
            {
                //如果能查找到节点。
                CustomerNode current = this._customerLookup[customerId];
                CustomerNode? start = null, end = null;
                CustomerNode? p = current;
                //1.指针上前移动high个数，且存在前节点，求得起始节点。
                if (p.Previous == null || high <= 0)
                {
                    start = current;
                }
                else
                {
                    while (high > 0 && p.Previous != null)
                    {
                        high--;
                        start = p = p.Previous;
                    }
                }
                //2.指针上后移动low个数，且存在后节点，求得终止节点。
                p = current;
                if (p.Next == null || low <= 0)
                {
                    end = current;
                }
                else
                {
                    while (low > 0 && p.Next != null)
                    {
                        low--;
                        end = p = p.Next;
                    }
                }
                p = start;
                //3.遍历开始及终止节点，返回结果。
                do
                {
                    if (p != null)
                    {
                        customers.Add(p.Customer);
                    }
                    if (p != end)
                    {
                        p = p.Next;
                    }
                    else
                    {
                        p = null;
                    }
                } while (p != null);

            }
            return customers;
        }
    }
}
