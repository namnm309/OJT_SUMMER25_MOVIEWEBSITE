using System;
using System.Collections.Generic;

namespace ApplicationLayer.DTO.ConcessionManagement
{
    public class ConcessionItemListDto
    {
        public List<ConcessionItemSummaryDto> Items { get; set; }
        public int TotalCount { get; set; }
    }

    public class ConcessionItemSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
