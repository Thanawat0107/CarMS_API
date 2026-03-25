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
            IMapper mapper,
            IFileUpload fileUpload)
        {
            _brandRepo = brandRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            // 🌟 เพิ่ม Filter ให้แสดงเฉพาะแบรนด์ที่ไม่ได้ถูกลบ (IsDeleted == false)
            var (brands, totalCount) = await _brandRepo.GetAllAsync(
                filter: q => !q.IsDeleted,
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
            
            // 🌟 เช็คด้วยว่าแบรนด์นี้ถูกลบไปแล้วหรือยัง
            if (brand == null || brand.IsDeleted) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณค้นหา หรือแบรนด์ถูกลบไปแล้ว"));
            
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] BrandCreateDto brandDto)
        {
            var brand = _mapper.Map<Brand>(brandDto);
            brand.IsDeleted = false; // 🌟 แก้ชื่อให้ตรงกับ Model
            brand.IsUsed = true; // สมมติว่าสร้างมาแล้วให้เปิดใช้งานเลย
            
            // อัปโหลดภาพ (แบรนด์มีแค่รูปเดียว)
            if (brandDto.ImageFile != null)
            {
                // 🌟 เปลี่ยนจาก CarImages เป็น CarImages
                brand.CarImages = await _fileUpload.UploadFile(brandDto.ImageFile, SD.ImgBrandPath);
            }

            var created  = await _brandRepo.AddAsync(brand);
            var result = _mapper.Map<BrandDto>(created);

            return Ok(ApiResponse<BrandDto>.Success(result, "สร้างแบรนด์สำเร็จ"));
        }

        [HttpPut("update/{brandId}")]
        public async Task<IActionResult> Update([FromForm] BrandUpdateDto brandDto, int brandId)
        {
            var brand = await _brandRepo.GetByIdAsync(brandId);
            if (brand == null || brand.IsDeleted) 
                return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณต้องการแก้ไข"));
            
            _mapper.Map(brandDto, brand);
            
            // อัปเดตภาพ
            if (brandDto.ImageFile != null)
            {
                // 🌟 เปลี่ยนจาก CarImages เป็น CarImages
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
            if (brand == null || brand.IsDeleted) 
                return NotFound(ApiResponse<string>.Fail($"ไม่พบแบรนด์ ID: {brandId}"));

            // 🌟 เอาคำสั่งลบไฟล์ภาพออก เพื่อให้สอดคล้องกับการทำ Soft Delete เผื่อกู้คืนข้อมูล
            brand.IsDeleted = true;
            await _brandRepo.UpdateAsync(brand);
            
            return Ok(ApiResponse<string>.Success("ลบแบรนด์เรียบร้อยแล้ว (Soft Delete)"));
        }
    }
}