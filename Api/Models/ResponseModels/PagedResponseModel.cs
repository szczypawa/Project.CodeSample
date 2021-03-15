using System;
using System.Collections.Generic;

namespace Project.Core.Api.V1.ResponseModels
{
    public class PagedResponseModel<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string NextPage { get; set; }
        public string PreviousPage { get; set; }

        public PagedResponseModel() { }

        public PagedResponseModel(IEnumerable<T> data) 
        {
            Data = data;
        }
    }
}
