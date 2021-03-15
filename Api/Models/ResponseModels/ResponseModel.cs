namespace Project.Core.Api.V1.ResponseModels
{
    public class ResponseModel<T>
    {
        public T Data { get; set; }

        public ResponseModel() { }

        public ResponseModel(T response) 
        {
            Data = response;
        }
    }
}
