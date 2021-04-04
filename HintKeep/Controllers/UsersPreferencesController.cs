using System;
using System.Collections.Generic;
using System.Linq;
using HintKeep.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HintKeep.Controllers
{
    [ApiController, Route("api/users/preferences")]
    public class UsersPreferencesController : ControllerBase
    {
        [AllowAnonymous, HttpGet]
        public IActionResult Get()
            => Ok(new UserPreferences
            {
                PreferredLanguages = _GetPreferredLanguages()
            });

        [AllowAnonymous, HttpGet("preferred-languages")]
        public IActionResult GetPreferredLanguages()
            => Ok(_GetPreferredLanguages());

        private IEnumerable<string> _GetPreferredLanguages()
            => StringWithQualityHeaderValue.TryParseList(Request.Headers.TryGetValue(HeaderNames.AcceptLanguage, out var acceptLanguageHeader) ? acceptLanguageHeader : Array.Empty<string>(), out var languagePreferences)
                ? languagePreferences
                    .Where(languagePreference => languagePreference.Value.Value != "*")
                    .OrderByDescending(languagePreference => languagePreference.Quality)
                    .Select(languagePreference => languagePreference.Value.Value)
                    .ToArray()
                : Enumerable.Empty<string>();
    }
}