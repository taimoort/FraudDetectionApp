using System;

namespace Shared.Contracts
{
    public class TransactionDto
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFraud { get; set; }
        public float FraudScore { get; set; }
        public string? FraudReason { get; set; }
        public string? SourceService { get; set; }  // e.g. "fraud-service"
        public string? Environment { get; set; }    // e.g. "dev" or "prod"
    }
}