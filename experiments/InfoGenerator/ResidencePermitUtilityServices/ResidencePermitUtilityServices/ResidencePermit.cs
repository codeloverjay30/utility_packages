namespace ResidencePermitUtilityServices;

public class ResidencePermit
{
    public string PermitNumber { get; private set; }
    public string Nationality { get; private set; }
    public DateTime IssuedDate { get; private set; }

    public ResidencePermit(
        string permitNumber,
        string nationality,
        DateTime issuedDate
    )
    {
        if (string.IsNullOrWhiteSpace(permitNumber))
        {
            throw new ArgumentException("Permit number cannot be empty.", nameof(permitNumber));
        }

        PermitNumber = permitNumber;
        Nationality = nationality;
        IssuedDate = issuedDate;
    }
}
