using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;

namespace CarMS_API.Repositorys.IRepositorys
{
    public interface IAuthRepository
    {
        public Task<RegisterResponse> RegisterAsync(RegisterDto model);
        public Task<LoginResponse> LoginAsync(LoginDto model);
    }
}
