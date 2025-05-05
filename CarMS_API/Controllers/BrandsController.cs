using AutoMapper;
using CarMS_API.Data;
using CarMS_API.Models;
using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.RequestHelpers;
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
        public BrandsController(IRepository<Brand> brandRepo, 
            ISearchableRepository<Brand, BrandSearchParams> searchRepo, 
            IMapper mapper)
        {
            _brandRepo = brandRepo;
            _searchRepo = searchRepo;
            _mapper = mapper;
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
        public async Task<IActionResult> Create(BrandDto brandDto)
        {
            var brand = _mapper.Map<Brand>(brandDto);
            brand.IsUsed = false;
            brand.IsDelete = false;

            var created  = await _brandRepo.AddAsync(brand);
            var result = _mapper.Map<BrandDto>(brand);

            return Ok(ApiResponse<BrandDto>.Success(result, "สำเร็จ"));
        }

        [HttpPut("{brandId}")]
        public async Task<IActionResult> Update(BrandDto brandDto)
        {
            var brand = await _brandRepo.GetByIdAsync(brandDto.Id);
            if (brand == null) return NotFound(ApiResponse<string>.Fail("ไม่พบแบรนด์ที่คุณต้องการแก้ไข"));

            _mapper.Map(brandDto, brand);
            await _brandRepo.UpdateAsync(brand);

            var result = _mapper.Map<BrandDto>(brand);
            return Ok(ApiResponse<BrandDto>.Success(result, "อัปเดตแบรนด์เรียบร้อย"));
        }

        [HttpDelete("{brandId}")]
        public async Task<IActionResult> Delete(int brandId)
        {
            var deleted = await _brandRepo.DeleteAsync(brandId);
            if (deleted == null) return NotFound(ApiResponse<string>.Fail($"ไม่พบแบรนด์ ID: {brandId}"));

            return Ok(ApiResponse<string>.Success("ลบแบรนด์เรียบร้อยแล้ว"));
        }
    }
}
