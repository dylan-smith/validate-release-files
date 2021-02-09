﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ValidateReleaseFiles
{
    class Program
    {
        static int Main(string[] args)
        {
            var repoPath = Directory.GetCurrentDirectory();
            var releasePath = Path.Combine(repoPath, "release_files");

            if (args.Length > 0)
            {
                releasePath = RemoveQuotes(args[0]);
            }

            if (args.Length > 1)
            {
                repoPath = RemoveQuotes(args[1]);
            }

            if (!Directory.Exists(releasePath))
            {
                Console.Error.WriteLine($"ERROR: Cannot find the release files directory [{releasePath}]");
                return 1;
            }

            if (!Directory.Exists(repoPath))
            {
                Console.Error.WriteLine($"ERROR: Cannot find the repository directory [{repoPath}]");
                return 1;
            }

            if (!ValidateFiles(releasePath, repoPath))
            {
                return 1;
            }

            return 0;
        }

        private static string RemoveQuotes(string arg)
        {
            return arg.Trim('\'', '\"');
        }

        private static bool ValidateFiles(string releasePath, string repoPath)
        {
            var releaseFiles = GetReleaseFiles(releasePath);

            foreach (var releaseFile in releaseFiles)
            {
                Console.WriteLine($"Checking file {releaseFile.filePath} [{releaseFile.hash}]...");

                var match = CheckFile(releaseFile, repoPath);
                
                if (string.IsNullOrWhiteSpace(match))
                {
                    Console.Error.WriteLine($"ERROR: Cannot find a file that matches both filename and checksum [{releaseFile.fileName}]");

                    return false;
                }

                Console.WriteLine($"MATCH FOUND! ({match})");
            }

            return true;
        }

        private static string CheckFile((string fileName, string filePath, string hash) releaseFile, string repoPath)
        {
            var files = Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in files)
            {
                if (Path.GetFileName(filePath) == releaseFile.fileName)
                {
                    if (filePath != releaseFile.filePath)
                    {
                        if (HashFile(filePath) == releaseFile.hash)
                        {
                            return filePath;
                        }
                    }
                }
            }

            return null;
        }

        private static IEnumerable<(string fileName, string filePath, string hash)> GetReleaseFiles(string releasePath)
        {
            var files = Directory.GetFiles(releasePath);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var hash = HashFile(filePath);

                yield return (fileName, filePath, hash);
            }
        }

        private static string HashFile(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);

            var hashBytes = md5.ComputeHash(stream);

            var sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
