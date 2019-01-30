namespace Utils
{
    public static class StringUtils
    {
        public static int ParticularCharCount(string str,char particularChar)
        {
            int count = 0;
            foreach (char c in str)
                if (c == particularChar) count++;
            return count;
        }
    }
}