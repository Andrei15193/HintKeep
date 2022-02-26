using System.Collections.Generic;

namespace HintKeep.ViewModels.Users
{
    public record UserPreferences(
        IEnumerable<string> PreferredLanguages
    );
}