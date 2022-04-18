namespace RanTodd
{
    public class Role
    {
        public string Id => $"{Guild}/{Level}";
        public ulong Guild { get; set; }
        public ulong Level { get; set; }
        public ulong RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
