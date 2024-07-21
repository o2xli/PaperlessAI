namespace PaperlessAI.Shared;

public static class Helper
{
    public static int CountWords(this ReadOnlySpan<char> input)
    {
        int wordCount = 0;
        while (!input.IsEmpty)
        {
            while (!input.IsEmpty && char.IsWhiteSpace(input[0]))
                input = input.Slice(1);

            if (!input.IsEmpty)
                wordCount++;

            while (!input.IsEmpty && !char.IsWhiteSpace(input[0]))
                input = input.Slice(1);
        }   

        return wordCount;
    }
}