namespace Flipit.Core
{
    public class PurchaseResult
    {
        public bool Success { get; private set; }
        public string FailReason { get; private set; }
        public ChipResult[] Chips { get; private set; }

        private PurchaseResult() { }

        public static PurchaseResult Succeeded(ChipResult[] chips)
        {
            return new PurchaseResult
            {
                Success = true,
                FailReason = null,
                Chips = chips
            };
        }

        public static PurchaseResult Failed(string reason)
        {
            return new PurchaseResult
            {
                Success = false,
                FailReason = reason,
                Chips = null
            };
        }
    }
}