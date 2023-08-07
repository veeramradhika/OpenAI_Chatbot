using System.ComponentModel.DataAnnotations;

namespace OpenAI_Chatbot.Models
{

    public class PromptModel
    {
        [Required(ErrorMessage = "Please choose an option.")]
        public string Option { get; set; }

        [Required(ErrorMessage = "Please enter a prompt.")]
        public string PromptText { get; set; }

        public string GeneratedResponse { get; set; }

        public string Email { get; set; }
    }

}
