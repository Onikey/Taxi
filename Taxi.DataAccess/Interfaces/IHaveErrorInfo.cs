namespace Taxi.DataAccess.Interfaces
{
    public interface IHaveErrorInfo
    {
        string ResultMessage { get; set; }

        bool HasError { get; set; }
    }
}
