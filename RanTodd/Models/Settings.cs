namespace RanTodd
{
    public class Settings
    {
        public ulong Guild { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }
        public string Banner { get; set; }
        public List<ulong> ExcludedChannels { get; set; }
    }
}
