﻿using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using Microsoft.AspNetCore.Mvc;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;

        public AuthController(IAuthRepository authRepo)
        {
            _authRepo = authRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authRepo.RegisterAsync(model);
            return Ok(ApiResponse<RegisterResponse>.Success(result, "สมัครสมาชิกสำเร็จ"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _authRepo.LoginAsync(model);
            return Ok(ApiResponse<LoginResponse>.Success(result, "เข้าสู่ระบบสำเร็จ"));
        }
    }
}
