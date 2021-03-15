using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Api.V1.ResponseModels
{
    public class SessionResponseModel
    {
        public int ClientId { get; set; }
        public int SessionId { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
