using System.IO.Abstractions;

namespace ResidencePermitUtilityServices;

public class InMemoryPermitRepository : IPermitRepository
{
    private readonly IFileSystem _fileSystem;
    public InMemoryPermitRepository(IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(fileSystem, nameof(fileSystem));
        _fileSystem = fileSystem;
    }
    public void Save(ResidencePermit permit,string path)
    {
        var text = permit.PermitNumber.ToString();
        _fileSystem.File.WriteAllText(path,text);
    }
}
    
