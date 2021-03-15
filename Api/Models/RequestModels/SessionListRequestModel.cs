using System.ComponentModel.DataAnnotations;

namespace Project.Core.Api.V1.RequestModels
{
    public class SessionListRequestModel
    {
        [Required]
        public int ClientId { get; set; }
    }
}
