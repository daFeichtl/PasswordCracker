using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordCracker;

public class Cracker
{
    private double _counter = 0;
    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();
        foreach (var t in bytes)
        {
            builder.Append(t.ToString("x2"));
        }
        return builder.ToString();
    }
    /*
    public static async Task<string> CrackAll(string hash, string alphabet, int length, int threadCount, CancellationToken? token)
    {
        var tasks = new List<Task<string>>();
        var prefix = GetCharsBetweenAlphabet(alphabet, threadCount);
        for (var i = 0; i < threadCount; i++)
        {
            var task = Crack(hash, alphabet, length, prefix[i], token);
            tasks.Add(task);
        }
        var results = await Task.WhenAll(tasks);
        return results.FirstOrDefault(x => x != null);
    }
*/
    private string Crack(string hash, 
        string alphabet, 
        int length, 
        string? prefix, 
        IProgress<double> progress, 
        CancellationToken? token)
    {
        var builder = new StringBuilder();
        if (prefix != null)
        {
            builder.Append(prefix);
        }
        for (var i = 0; i < length; i++)
        {
            builder.Append(alphabet[0]);
        }
        var password = builder.ToString();
        List<string> allPws = new();
        var passwordHash = ComputeSha256Hash(password);
        var index = 0;
        while (passwordHash != hash)
        {
            if (token?.IsCancellationRequested ?? false)
            {
                return "";
            }
            password = Increment(password, alphabet, index);
            allPws.Add(password);
            passwordHash = ComputeSha256Hash(password);
            index = password.Length - 1;
            if (password.Length > length+1)
            {
                password = "";
                break;
            }

            if (_counter >= 1000)
            {
                progress.Report(_counter);
                _counter = 0;
            }

            _counter++;
        }
        return password;
    }

    private static string Increment(string password, string alphabet, int index)
    {
        var builder = new StringBuilder(password);
        var charIndex = alphabet.IndexOf(password[index]);
        if (charIndex == alphabet.Length - 1)
        {
            builder[index] = alphabet[0];
            if (index > 0)
            {
                builder = new StringBuilder(Increment(builder.ToString(), alphabet, index - 1));
            }
            else
            {
                builder.Insert(0, alphabet[0]);
            }
        }
        else
        {
            builder[index] = alphabet[charIndex + 1];
        }
        return builder.ToString();
    }

    public static string[] GetPrefixes(string alphabet, int threadCount)
    {
        var tmp = alphabet.Length / threadCount;
        var strings = new string[threadCount];
        for (var i = 0; i < threadCount; i++)
        {
            strings[i] = alphabet.Substring(i * tmp, 1);
        }

        return strings;
    }

    public Task<string> CrackAsync(string hash,
        string alphabet,
        int length,
        string? prefix,
        IProgress<double> progress,
        CancellationToken? token)
    {
        return Task.Run(() => Crack(hash, alphabet, length, prefix, progress, token));
    }

    public Task<Task<string>> MultiCrackAsync(string hash,
        string alphabet,
        int length,
        IProgress<double> progress,
        int threadCount,
        CancellationToken? token)
    {
        List<Task<string>> trds = new();
        var prefixes = GetPrefixes(alphabet, threadCount);
        for (int i = 0; i < threadCount; i++)
        {
            var cracker = CrackAsync(hash, alphabet, length, prefixes[i], progress, token);
            trds.Add(cracker);
        }

        return Task.WhenAny(trds);
    }
    
}