﻿namespace CarMS_API.RequestHelpers
{
    public class SellerSearchParams
    {
        public string? UserSearchTerm { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Address { get; set; }
        public bool? IsVerified { get; set; }

        public string? SortBy { get; set; } = "id";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
