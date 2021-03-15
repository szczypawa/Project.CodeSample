using Microsoft.AspNetCore.Mvc;

namespace Project.Core.Api.V1.RequestModels.Queries
{
    public class GetAllClientsQuery
    {
        [FromQuery(Name = "term")]
        public string Term { get; set; }
    }
}
