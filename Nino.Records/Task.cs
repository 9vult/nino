
namespace Nino.Records
{
    public record Task
    {
        public required string Abbreviation;
        public required bool Done;
        public DateTimeOffset? Updated;
    }
}
