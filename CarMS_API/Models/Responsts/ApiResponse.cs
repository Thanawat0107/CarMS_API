namespace CarMS_API.Models.Responsts
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
        public T? Result { get; init; }
        public object? Meta { get; init; } // ใช้ได้ทั้ง pagination หรืออื่นๆ

        public static ApiResponse<T> Success(T result, string? message = null, object? meta = null)
            => new() { IsSuccess = true, Result = result, Message = message, Meta = meta };

        public static ApiResponse<T> Fail(string message)
            => new() { IsSuccess = false, Message = message };
    }
}
