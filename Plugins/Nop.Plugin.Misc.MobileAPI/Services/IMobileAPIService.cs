using System.Collections.Generic;

namespace Nop.Plugin.Misc.MobileAPI.Services
{
    public partial interface IMobileAPIService
    {
        int getLanguageIdByCulture(string culture);
    }
}
