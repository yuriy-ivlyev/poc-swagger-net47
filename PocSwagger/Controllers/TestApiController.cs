using System.Web.Http;
using PocSwagger.Attributes;
using PocSwagger.Models;

namespace PocSwagger.Controllers
{
    /// <summary>
    /// Test API to demonstrate consumer-based Swagger segmentation.
    /// </summary>
    [RoutePrefix("api/test")]
    public class TestApiController : ApiController
    {
        /// <summary>
        /// Method1 – available only for consumer1.
        /// Accepts a complex Consumer1Request that includes shared types
        /// (ContactInfo, Address, Priority).
        /// </summary>
        [HttpPost]
        [Route("method1")]
        [ApiConsumer("consumer1")]
        public Consumer1Response Method1([FromBody] Consumer1Request request)
        {
            return new Consumer1Response
            {
                Id = 1,
                Result = $"Hello {request?.Name}",
                EchoedContact = request?.Contact
            };
        }

        /// <summary>
        /// Method2 – available for both consumer1 and consumer2.
        /// Accepts a SharedRequest that includes shared types (DateRange, Priority).
        /// </summary>
        [HttpPost]
        [Route("method2")]
        [ApiConsumer("consumer1", "consumer2")]
        public SharedResponse Method2([FromBody] SharedRequest request)
        {
            return new SharedResponse
            {
                TotalCount = 1,
                Items = new[] { request?.Query ?? "default" },
                AppliedDateRange = request?.DateRange
            };
        }

        /// <summary>
        /// Method3 – available only for consumer2.
        /// Accepts a complex Consumer2Request that includes shared types
        /// (Address, DateRange, Priority).
        /// </summary>
        [HttpPost]
        [Route("method3")]
        [ApiConsumer("consumer2")]
        public Consumer2Response Method3([FromBody] Consumer2Request request)
        {
            return new Consumer2Response
            {
                TransactionId = "TXN-001",
                Success = request?.Amount > 0,
                EchoedBillingAddress = request?.BillingAddress
            };
        }
    }
}
