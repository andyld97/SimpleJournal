namespace SimpleJournal.Common.Extensions
{
    public static class StringExtensions
    {
        public static bool AreEqual(this string str1, string str2, bool ignoreCase = true)
        {
            if (str1 == str2)
                return true;

            if (ignoreCase)
                return str1.ToLower().Contains(str2.ToLower());
            else
                return str1.Contains(str2);
        }
    }
}