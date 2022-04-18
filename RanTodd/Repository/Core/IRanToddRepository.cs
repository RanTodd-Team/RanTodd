namespace RanTodd
{
    public interface IRanToddRepository
    {
        IRanksRepository Ranks { get; }

        IRolesRepository Roles { get; }

        ISettingsRepository Settings { get; }
    }
}
