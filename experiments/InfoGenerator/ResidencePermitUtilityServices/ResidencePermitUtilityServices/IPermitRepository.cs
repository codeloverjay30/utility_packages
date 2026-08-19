namespace ResidencePermitUtilityServices;

public interface IPermitRepository
{
    void Save(ResidencePermit permit,string path);
}
