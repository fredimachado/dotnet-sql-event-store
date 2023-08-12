namespace WareHouseApi.Domain;

public class InvalidDomainException : Exception
{
    public InvalidDomainException(string message) : base(message)
    {
    }
}
