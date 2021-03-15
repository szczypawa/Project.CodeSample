using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Api.V1.RequestModels
{
    public class RefreshTokenRequestModel
    {
        public string authToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
