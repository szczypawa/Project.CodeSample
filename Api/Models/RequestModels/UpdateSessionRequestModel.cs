using System.ComponentModel.DataAnnotations;

namespace Project.Core.Api.V1.RequestModels
{
    public class UpdateSessionRequestModel
    {
        [Required]
        public long SessionId { get; set; }

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
