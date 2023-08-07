using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ChatBotMvc.Controllers
{
    public class HomeController : Controller
    {
        const string API_KEY = "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";
        private readonly ILogger<HomeController> _logger;
        static readonly HttpClient client = new HttpClient();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Get(string option, string prompt)
        {
            ViewBag.UserOption = option;
            ViewBag.Prompt = prompt;

            var options = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = option
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                max_tokens = 3500,
                temperature = 0.2
            };

            var json = JsonConvert.SerializeObject(options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", API_KEY);

            try
            {
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                string result = jsonResponse.choices[0].message.content;

                ViewBag.Response = result; // Store the response in ViewBag

                return View("Get"); // Return the Get view to display the generated response
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(string response, string email)
        {
            try
            {
                // Encode the response to HTML entities
                var encodedResponse = WebUtility.HtmlEncode(response);

                // Create a new email message
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Radhika", "veeramradhika@gmail.com")); //  your email
                message.To.Add(new MailboxAddress("Recipient", email)); // Use MailboxAddress constructor with display name and email address
                message.Subject = "Generated Response from ChatBot";

                // Create the plain-text version of the message body
                var plainTextBody = response;

                // Create the HTML version of the message body
                var htmlBody = $"<pre>{encodedResponse}</pre>";

                // Create the plain-text and HTML versions of the message
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = plainTextBody;
                bodyBuilder.HtmlBody = htmlBody;

                message.Body = bodyBuilder.ToMessageBody();

                // Send the email using MailKit with your SMTP settings
                using var client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false); // Replace with your SMTP server details
                await client.AuthenticateAsync("veeramradhika@gmail.com", "pborjoxdavbumpmu"); // Replace with your email and password
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Handle email sending errors here
                return Json(ex.Message);
            }

            ViewBag.EmailSent = true;
            return View("Get");
        }

    }
}
