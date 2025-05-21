using AutoMapper;
using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Dto.ViewModelDto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
using CarMS_API.Services.IServices;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IRepository<Brand> _brandRepo;
        private readonly ISearchableRepository<Brand, BrandSearchParams> _searchRepo;
        private readonly IMapper _mapper;
        private readonly IFileUpload _fileUpload;
        public BrandsController(IRepository<Brand> brandRepo, 
            ISearchableRepository<Brand, BrandSearchParams> searchRepo, 
            IMapper mapper
            ,IFileUpload fileUpload)
        {
            _brandRepo = brandRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
            _fileUpload = fileUpload;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BrandSearchParams searchParams)
        {
            var filter = _searchRepo.BuildFilter(searchParams);
            var orderBy = _searchRepo.BuildSort(searchParams.SortBy);

            var (brands, totalCount) = await _brandRepo.GetAllAsync(
                filter,
                orderBy,
                _searchRepo.Include(),
                searchParams.PageNumber,
                searchParams.PageSize
            );

            var result = _mapper.Map<IEnumerable<BrandDto>>(brands);

            var pagination = new PaginationMeta
            {
                TotalCount = totalCount,
                PageNumber = searchParams.PageNumber,
                PageSize = searchParams.PageSize
            };

            return Ok(ApiResponse<IEnumerable<BrandDto>>.Success(result, "โหลดรายการแบรนด์สำเร็จ", pagination));
        }

        [HttpGet("{brandId}")]
        public async Task<IActionResult> GetById(int brandId)
        {
            var brand = await _brandRepo.GetByIdAsync(brandId);
            if (brand == null) return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณค้นหา"));
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BrandDto brandDto)
        {
            var brand = _mapper.Map<Brand>(brandDto);
            brand.IsUsed = false;
            brand.IsDelete = false;
            
            // อัปโหลดภาพ
            if (brandDto.ImageFile != null)
            {
                brand.ImageUrl = await _fileUpload.UploadFile(brandDto.ImageFile, SD.ImgBrandPath);
            }

            var created  = await _brandRepo.AddAsync(brand);
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{brandId}")]
        public async Task<IActionResult> Update([FromForm] BrandDto brandDto)
        {
            var brand = await _brandRepo.GetByIdAsync(brandDto.Id);
            if (brand == null) return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณต้องการแก้ไข"));

            _mapper.Map(brandDto, brand);
            
            // อัปเดตภาพ
            if (brandDto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(brand.ImageUrl))
                {
                    _fileUpload.DeleteFile(brand.ImageUrl);
                }
                brand.ImageUrl = await _fileUpload.UploadFile(brandDto.ImageFile, SD.ImgBrandPath);
            }

            await _brandRepo.UpdateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(ApiResponse<BrandDto>.Success(result, "อัปเดตแบรนด์เรียบร้อย"));
        }

        [HttpPut("{Id}")]
        public async Task<IActionResult> Delete(int Id)
        {
            var brand = await _brandRepo.GetByIdAsync(Id);
            if (brand == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบแบรนด์ ID: {Id}"));

            // ลบไฟล์ภาพก่อนลบสินค้า
            if (!string.IsNullOrEmpty(brand.ImageUrl))
            {
                _fileUpload.DeleteFile(brand.ImageUrl);
            }

            brand.IsDelete = true;
            await _brandRepo.UpdateAsync(brand);
            return Ok(ApiResponse<string>.Success("ลบแบรนด์เรียบร้อยแล้ว"));
        }

    }
}
