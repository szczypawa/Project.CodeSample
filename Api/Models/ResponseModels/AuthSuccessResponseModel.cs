namespace Project.Core.Api.V1.ResponseModels
{
    public class AuthSuccessResponseModel
    {
        public string authToken { get; set; }
        public string refreshToken { get; set; }
    }
}
