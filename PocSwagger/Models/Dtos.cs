namespace PocSwagger.Models
{
    // ── Consumer1 DTOs ──────────────────────────────────────────────────────────

    public class Consumer1Request
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class Consumer1Response
    {
        public int Id { get; set; }
        public string Result { get; set; }
    }

    // ── Consumer2 DTOs ──────────────────────────────────────────────────────────

    public class Consumer2Request
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }

    public class Consumer2Response
    {
        public string TransactionId { get; set; }
        public bool Success { get; set; }
    }

    // ── Shared DTOs (consumer1 + consumer2) ─────────────────────────────────────

    public class SharedRequest
    {
        public string Query { get; set; }
        public int PageSize { get; set; }
    }

    public class SharedResponse
    {
        public int TotalCount { get; set; }
        public string[] Items { get; set; }
    }
}
