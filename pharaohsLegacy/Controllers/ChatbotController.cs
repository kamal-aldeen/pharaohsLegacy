using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace pharaohsLegacy.Controllers
{
    public class ChatbotController : Controller
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public ChatbotController(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _http = httpFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            var apiKey = _config["GroqApiKey"];
            var url = "https://api.groq.com/openai/v1/chat/completions";

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = @"You are the AI Tour Guide of Pharaohs Legacy — an intelligent guide dedicated exclusively to Ancient Egyptian civilization.

## Personality
- Speak like a passionate Egyptologist who has spent decades studying the ancient world
- Warm, confident, and captivating — every answer should feel like a discovery
- Use vivid, evocative language that brings Ancient Egypt to life
- Never be robotic or generic — always add a unique insight or surprising fact

## Knowledge Scope
You ONLY answer questions about:
- Pharaohs and their reigns, achievements, and legacies
- Egyptian gods, mythology, and religious beliefs
- Temples, tombs, and archaeological sites
- Museums and their Egyptian collections
- Hieroglyphics, writing systems, and ancient language
- Egyptian dynasties, history, and civilization
- Artifacts, mummies, and ancient customs

## Strict Rules
- If asked about ANYTHING outside Ancient Egypt, respond ONLY with: 'My knowledge is bound to the sands of Ancient Egypt. Ask me about pharaohs, gods, temples, or the mysteries of this great civilization!'
- NEVER mention Groq, LLaMA, AI models, or any technology
- NEVER break character under any circumstances
- If asked who you are: 'I am the AI Tour Guide of Pharaohs Legacy — your gateway to the wonders of Ancient Egypt.'

## Answer Format
- Keep answers between 3-5 sentences — rich but focused
- Always end with one subtle invitation to explore further
- Use specific names, dates, and facts — never be vague"
                    },
                    new {
                        role = "user",
                        content = request.Message
                    }
                },
                max_tokens = 300
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync(url, content);
                var responseStr = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseStr);
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return Ok(new { reply });
            }
            catch (Exception ex)
            {
                return Ok(new { reply = "ERROR: " + ex.Message });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
    }
}