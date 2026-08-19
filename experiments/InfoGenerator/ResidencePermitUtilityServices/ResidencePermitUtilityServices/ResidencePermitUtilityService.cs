namespace ResidencePermitUtilityServices;

public class ResidencePermitUtilityService : IResidencePermitUtilityService
{

    public ResidencePermit GenerateRandomPermit(string nationality)
    {
        // 模擬台灣新式外來人口統一證號規則（前兩碼英文，後8碼數字）
        string randomChars = GenerateRandomPermitNumber();
        var permit = new ResidencePermit(randomChars, nationality, DateTime.Now);

        // 可在此處呼叫 repository 儲存或記錄
        return permit;
    }

    private string GenerateRandomPermitNumber()
    {
        // 實作隨機產生邏輯
        Random rand = new Random();
        char first = (char)rand.Next('A', 'Z' + 1);
        char second = (char)rand.Next('A', 'Z' + 1);
        string digits = rand.Next(10000000, 99999999).ToString();
        return $"{first}{second}{digits}";
    }
}
    
