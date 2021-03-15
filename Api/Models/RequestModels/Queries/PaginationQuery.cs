using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.Api.V1.RequestModels.Queries
{
    public class PaginationQuery
    {
        [FromQuery(Name = "pageNumber")]
        public int PageNumber { get; set; }
        [FromQuery(Name = "pageSize")]
        public int PageSize { get; set; }

        public PaginationQuery() 
        {
            PageNumber = 1;
            PageSize = 5;
        }
        public PaginationQuery(int pageNumber, int pageSize) 
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;

            if (pageSize > 100)
            {
                PageSize = 100;
            }
            else if (pageSize < 1)
            {
                PageSize = 1;
            }
            else
            {
                PageSize = pageSize;
            }
        }
    }
}
