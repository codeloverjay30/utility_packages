using NReco.Text;

namespace DataStructureUtilityServices;

public class AhoCorasickDoubleArrayTrieMatcher: IAhoCorasickDoubleArrayTrieMatcher
{
    private readonly AhoCorasickDoubleArrayTrie<string> _trie;


    /// <summary>
    /// check the <paramref name="text"/> appears in <see cref="global::DataStructureUtilityServices.AhoCorasickDoubleArrayTrieMatcher._trie" />
    /// exactly <paramref name="times" /> times.
    /// </summary>
    public bool IsExactlyNTimes(
        ReadOnlySpan<char> text,
        int times
    )
    {
        if (text.IsEmpty)
        {
            throw new ArgumentException("text cannot be empty", nameof(text));
        }
        
        int matchCount = 0;
        bool isGreaterThanNTimes = false;
        string textString = text.ToString(); // 邊界轉換


        // 開始掃描
        _trie.ParseText(textString, (hit) =>
        {
            matchCount++;

            // 防禦性設計 (Short-circuit)：
            // 一旦發現出現times次以上，就代表不符合「恰好time」的條件。
            // 雖然 NReco 無法直接從內部 return Break，但我們可以透過條件控制減少後續處理。
            if (matchCount > times)
            {
                isGreaterThanNTimes = true;
            }
        });

        // 恰好等於 times 次才回傳 true
        return !isGreaterThanNTimes && matchCount == times;
    }

    public bool IsExactlyOnce()
    {
        
    }
}
