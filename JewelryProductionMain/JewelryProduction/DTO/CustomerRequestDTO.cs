﻿namespace JewelryProduction.DTO
{
    public class CustomerRequestDTO
    {
        public string Type { get; set; } = null!;

        public string Style { get; set; } = null!;

        public double? Size { get; set; }

        public decimal Quantity { get; set; }
        public string GoldType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public AddGemstoneDTO PrimaryGemstone { get; set; } = new AddGemstoneDTO();
        public List<string> AdditionalGemstoneNames { get; set; } = new List<string>();
    }
}
