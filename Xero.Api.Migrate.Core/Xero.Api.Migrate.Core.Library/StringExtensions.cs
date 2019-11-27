using System;

namespace Xero.Api.Migrate.Core.Library
{
    public static class StringExtensions
    {
        public static string Escape(this string source)
        {
            return Uri.EscapeDataString(source);
        }
    }
}