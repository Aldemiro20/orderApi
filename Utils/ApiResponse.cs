namespace orderApi.Utils
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public int StatusCode { get; set; } 

        public static ApiResponse SuccessResponse(object data = null, string message = "Operação bem sucedida")
        {
            return new ApiResponse { Success = true, Data = data, Message = message, StatusCode = 200 }; 
        }

        public static ApiResponse ErrorResponse(string message = "Ocorreu um erro", int statusCode = 400)
        {
            return new ApiResponse { Success = false, Message = message, StatusCode = statusCode }; 
    }
}
