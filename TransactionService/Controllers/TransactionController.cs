using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using System.Text.Json;
using System.Threading.Tasks;

namespace TransactionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly KafkaProducerService _producer;

        public TransactionController(KafkaProducerService producer)
        {
            _producer = producer;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TransactionDto tx)
        {
            // Serialize the DTO to JSON
            var message = JsonSerializer.Serialize(tx);

            // Publish to the "transactions" Kafka topic
            await _producer.SendMessageAsync("transactions", message);

            return Ok(new { status = "sent", tx.TransactionId });
        }
    }
}