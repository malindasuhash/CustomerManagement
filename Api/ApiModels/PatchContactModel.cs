namespace Api.ApiModels
{
    // Some considerations:
    // 1. Properties may need to optional so that the serialisation framework
    // can ignore properties without marking them mandatory.
    // 2. Target version is vital as it maps to latest draft version stored.
    public class PatchContactModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public int TargetVersion { get; set; } = 0;
    }
}
