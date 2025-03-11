namespace Localizer
{
    internal class LocalizationException : Exception
    {
        public LocalizationException() { }

        public LocalizationException(string message) : base(message) { }

        public LocalizationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
