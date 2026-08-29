using System;
using System.Collections.Generic;

namespace PocSwagger.Models
{
    // ── Shared value types (used by both Consumer1 and Consumer2) ────────────────

    /// <summary>Priority level shared across consumers.</summary>
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>Postal address shared across consumers.</summary>
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
    }

    /// <summary>Contact information shared across consumers.</summary>
    public class ContactInfo
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public Address Address { get; set; }
    }

    /// <summary>Inclusive date range shared across consumers.</summary>
    public class DateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    // ── Consumer1 DTOs ──────────────────────────────────────────────────────────

    /// <summary>Request payload for Consumer1 operations.</summary>
    public class Consumer1Request
    {
        public string Name { get; set; }
        public int Value { get; set; }

        /// <summary>Priority of the request – shared with Consumer2.</summary>
        public Priority Priority { get; set; }

        /// <summary>Contact details – shared type also used by Consumer2.</summary>
        public ContactInfo Contact { get; set; }

        /// <summary>Optional tags.</summary>
        public List<string> Tags { get; set; }
    }

    public class Consumer1Response
    {
        public int Id { get; set; }
        public string Result { get; set; }
        public ContactInfo EchoedContact { get; set; }
    }

    // ── Consumer2 DTOs ──────────────────────────────────────────────────────────

    /// <summary>Request payload for Consumer2 operations.</summary>
    public class Consumer2Request
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }

        /// <summary>Priority of the transaction – shared with Consumer1.</summary>
        public Priority Priority { get; set; }

        /// <summary>Billing address – shared type also used by Consumer1.</summary>
        public Address BillingAddress { get; set; }

        /// <summary>Period the transaction applies to – shared type also used in SharedRequest.</summary>
        public DateRange Period { get; set; }
    }

    public class Consumer2Response
    {
        public string TransactionId { get; set; }
        public bool Success { get; set; }
        public Address EchoedBillingAddress { get; set; }
    }

    // ── Shared DTOs (consumer1 + consumer2) ─────────────────────────────────────

    public class SharedRequest
    {
        public string Query { get; set; }
        public int PageSize { get; set; }

        /// <summary>Restrict results to this date range – shared type.</summary>
        public DateRange DateRange { get; set; }

        /// <summary>Minimum priority to include in results – shared enum.</summary>
        public Priority MinPriority { get; set; }
    }

    public class SharedResponse
    {
        public int TotalCount { get; set; }
        public string[] Items { get; set; }
        public DateRange AppliedDateRange { get; set; }
    }
}
