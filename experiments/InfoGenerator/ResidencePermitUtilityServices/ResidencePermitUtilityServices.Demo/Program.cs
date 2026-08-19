using System.IO.Abstractions;
using System.Reflection;
using ResidencePermitUtilityServices;

var path = Assembly.GetExecutingAssembly().FullName;

ResidencePermitUtilityService residencePermitUtilityService = new();

for(int i=0;i<10;i++){
var residencePermit =  residencePermitUtilityService.GenerateRandomPermit("Tawain");
//IFileSystem realFileSystem = new FileSystem();

//InMemoryPermitRepository inMemoryPermitRepository = new(realFileSystem);

Console.WriteLine(residencePermit.PermitNumber);
//inMemoryPermitRepository.Save(residencePermit, path);
}
Console.ReadLine();