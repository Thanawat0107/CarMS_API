using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;

namespace CarMS_API.Repositorys.IRepositorys
{
    public interface IAuthRepository
    {
        public Task<ApiResponse<object>> RegisterAsync(RegisterDto model);
        public Task<ApiResponse<LoginResponse>> LoginAsync(LoginDto model);
    }
}
