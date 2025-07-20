namespace SampleApp;

using SampleApp.Contracts;

public class SampleService(IDateTimeProvider dateTimeProvider) : ISampleService
{
    public string Greet(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        var now = dateTimeProvider.Now;
        return $"Hello, {name}. It's {now:HH:mm}.";
    }
}
