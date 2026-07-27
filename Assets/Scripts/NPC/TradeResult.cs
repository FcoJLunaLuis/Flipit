namespace Flipit.NPC
{
    /// <summary>
    /// Result of a trade operation.
    /// </summary>
    public class TradeResult
    {
        public bool Success { get; private set; }
        public string FailReason { get; private set; }
        public string ChipObtained { get; private set; }

        private TradeResult() { }

        public static TradeResult Succeeded(string chipObtained)
        {
            return new TradeResult
            {
                Success = true,
                FailReason = null,
                ChipObtained = chipObtained
            };
        }

        public static TradeResult Failed(string reason)
        {
            return new TradeResult
            {
                Success = false,
                FailReason = reason,
                ChipObtained = null
            };
        }
    }
}
