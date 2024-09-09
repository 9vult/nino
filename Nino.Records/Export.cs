namespace Nino.Records
{
    public record Export
    {
        public required Project Project;
        public required Episode[] Episodes;
    }
}
