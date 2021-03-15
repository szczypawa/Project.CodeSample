using System.ComponentModel.DataAnnotations;

namespace Project.Core.Api.V1.RequestModels
{
    public class CreateSessionRequestModel
    {
        [Required]
        public int ClientId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ImagedataFront { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ImagedataLeft { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ImagedataBack { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ImagedataRight { get; set; }
    }
}
