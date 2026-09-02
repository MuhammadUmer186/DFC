namespace RestaurantSystem.Models
{
    // Phase 3 — safe order numbering. Additive partial fragment.
    public partial class Order
    {
        /// <summary>
        /// <c>POS</c> | <c>WEB</c> | <c>CLD</c> for orders numbered by the
        /// per-branch/source/day sequence. <c>null</c> for orders created before
        /// Phase 3 (their legacy <see cref="OrderNumber"/> is left untouched and
        /// is excluded from the new uniqueness constraint).
        /// </summary>
        public string? OrderNumberSource { get; set; }
    }
}
