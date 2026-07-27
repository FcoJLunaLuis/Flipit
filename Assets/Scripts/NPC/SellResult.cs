namespace Flipit.NPC
{
    /// <summary>
    /// Result of a sell operation.
    /// </summary>
    public class SellResult
    {
        public bool Success { get; private set; }
        public string FailReason { get; private set; }
        public int MoneyEarned { get; private set; }
        public string ChipId { get; private set; }
        public int QuantitySold { get; private set; }

        private SellResult() { }

        public static SellResult Succeeded(string chipId, int quantity, int moneyEarned)
        {
            return new SellResult
            {
                Success = true,
                FailReason = null,
                MoneyEarned = moneyEarned,
                ChipId = chipId,
                QuantitySold = quantity
            };
        }

        public static SellResult Failed(string reason)
        {
            return new SellResult
            {
                Success = false,
                FailReason = reason,
                MoneyEarned = 0,
                ChipId = null,
                QuantitySold = 0
            };
        }
    }
}
