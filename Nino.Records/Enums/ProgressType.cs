using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ProgressType
    {
        Done = 0,
        Undone = 1,
        Skipped = 2,
    }

    public static class ProgressTypeExtensions
    {
        public static string ToFriendlyString(this ProgressType type, string lng)
        {
            return type switch
            {
                ProgressType.Done => T("choice.progress.type.done", lng),
                ProgressType.Undone => T("choice.progress.type.undone", lng),
                ProgressType.Skipped => T("choice.progress.type.skipped", lng),
                _ => type.ToString(),
            };
        }
    }
}
