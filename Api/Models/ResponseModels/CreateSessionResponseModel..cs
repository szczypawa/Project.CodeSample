using System;

namespace Project.Core.Api.V1.ResponseModels
{
    public class CreateSessionResponseModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImagePath { get; set; }
        public string ClientNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
