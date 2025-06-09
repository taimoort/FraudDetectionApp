using System;

public class FlaggedTransaction
{
    public int Id { get; set; }
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime FlaggedAt { get; set; }
}