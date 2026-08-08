
using RestaurantSystem.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantSystem.Printing
{
    public static class EscPosReceiptBuilder
    {
        private const string SEP = "==============================================";

        public static string Build(PrintReceiptDto dto)
        {
            var lines = new List<string>();

            // ===== RESET PRINTER =====
            lines.Add("\x1B\x40");

            // ===== HEADER =====
            lines.Add("\x1B\x61\x01\x1B\x45\x01\x1D\x21\x11" + $"DATA FINGER CHIPS" +
            "\x1D\x21\x00\x1B\x45\x00"); // DOUBLE SIZE
            //lines.Add("\x1B\x61\x01");
            lines.Add("Dream Mall Shop# 01 Lalarukh WahCantt");
            lines.Add("Cell # 0301-5637195");

            // ===== COPY TYPE =====
            lines.Add(dto.CopyType == "customer" ? "CUSTOMER COPY" : "KITCHEN COPY");

            // ===== ORDER INFO =====
            lines.Add("\x1B\x61\x00"); // LEFT

            lines.Add("\x1B\x61\x00\x1B\x45\x01\x1D\x21\x11" +
          $"ORDER NO: {dto.OrderNo}" + "\x1D\x21\x00\x1B\x45\x00");


            lines.Add($"Date: {DateTime.Now}");
            lines.Add(SEP);

            // ===== ITEMS =====
            foreach (var item in dto.Items)
            {
                string name = item.Name ?? string.Empty;
                lines.Add(name);

                string rate = item.Price.ToString("0.00").PadRight(8);
                string qty = item.Quantity.ToString().PadRight(8);
                string val = (item.Price * item.Quantity).ToString("0.00").PadLeft(8);

                lines.Add($"Rate: {rate} Qty: {qty} Val: Rs {val}");
                //lines.Add("");
            }

            lines.Add(SEP);

            // ===== TOTAL CALC =====
            decimal itemsTotal = dto.Items.Sum(i => i.Price * i.Quantity);
            decimal discountAmount = dto.Discount;
            decimal finalTotal = dto.FinalTotal;

            if (discountAmount > 0)
            {
                lines.Add($"Subtotal: Rs {itemsTotal:0.00}");
                lines.Add($"Discount: -Rs {discountAmount:0.00}");
                lines.Add(SEP);
            }

            // ===== BIG TOTAL =====
            lines.Add("\x1B\x45\x01\x1D\x21\x11" + $"TOTAL: Rs {finalTotal:0.00}" +
                "\x1D\x21\x00\x1B\x45\x00"); // BOLD


            // ===== FOOTER =====
            if (dto.CopyType == "customer")
            {

                lines.Add("\x1B\x61\x01"); // CENTER
                lines.Add("Thank you for your order!");
                lines.Add("Orbionix Technologies");
                lines.Add("Cell: 0328-5107458, 0303-5184773");
                lines.Add("\x1B\x61\x00"); // LEFT
            }

            // ===== FEED + CUT =====
            lines.Add("\n\n\n\x1D\x56\x00");

            return string.Join("\n", lines);
        }
    }
}