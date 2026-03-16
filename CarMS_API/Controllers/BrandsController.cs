using AutoMapper;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.CreateDto;
using CarMS_API.Models.Dto.UpdaeteDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Mvc;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepo;
        private readonly IMapper _mapper;
        private readonly IFileUpload _fileUpload;
        public BrandsController(IRepository<Brand> brandRepo, 
            IMapper mapper
            ,IFileUpload fileUpload)
        {
            _brandRepo = brandRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var (brands, totalCount) = await _brandRepo.GetAllAsync(
                pageNumber: pageNumber,
                pageSize: pageSize
            );

            var result = _mapper.Map<IEnumerable<BrandDto>>(brands);

            var meta = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(ApiResponse<IEnumerable<BrandDto>>.Success(result, "โหลดรายการแบรนด์สำเร็จ", meta));
        }

        [HttpGet("getbyid/{brandId}")]
        public async Task<IActionResult> GetById(int brandId)
        {
            var brand = await _brandRepo.GetByIdAsync(brandId);
            if (brand == null) return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณค้นหา"));
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] BrandCreateDto brandDto)
        {
            var brand = _mapper.Map<Brand>(brandDto);
            brand.IsDelete = false;
            
            // อัปโหลดภาพ
            if (brandDto.ImageFile != null)
            {
                brand.CarImages = await _fileUpload.UploadFile(brandDto.ImageFile, SD.ImgBrandPath);
            }

            var created  = await _brandRepo.AddAsync(brand);
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("update/{brandId}")]
        public async Task<IActionResult> Update([FromForm] BrandUpdateDto brandDto, int brandId)
        {
            var brand = await _brandRepo.GetByIdAsync(brandId);
            if (brand == null) return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณต้องการแก้ไข"));
            _mapper.Map(brandDto, brand);
            
            // อัปเดตภาพ
            if (brandDto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(brand.CarImages))
                {
                    _fileUpload.DeleteFile(brand.CarImages);
                }
                brand.CarImages = await _fileUpload.UploadFile(brandDto.ImageFile, SD.ImgBrandPath);
            }

            await _brandRepo.UpdateAsync(brand);
            var result = _mapper.Map<BrandDto>(brand);
            return Ok(ApiResponse<BrandDto>.Success(result, "อัปเดตแบรนด์เรียบร้อย"));
        }

        [HttpPut("delete/{brandId}")]
        public async Task<IActionResult> Delete(int brandId)
        {
            var brand = await _brandRepo.GetByIdAsync(brandId);
            if (brand == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบแบรนด์ ID: {brandId}"));

            // ลบไฟล์ภาพก่อนลบสินค้า
            if (!string.IsNullOrEmpty(brand.CarImages))
            {
                _fileUpload.DeleteFile(brand.CarImages);
            }

            brand.IsDelete = true;
            await _brandRepo.UpdateAsync(brand);
            return Ok(ApiResponse<string>.Success("ลบแบรนด์เรียบร้อยแล้ว"));
        }

    }
}
