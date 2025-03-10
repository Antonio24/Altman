﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Altman.Common.AltException;

namespace Altman.Common.AltData
{
    public class ResultMatch
    {
        public static string GetResultFromInterval(byte[] resultBytes, Encoding encode)
        {
            string result = encode.GetString(resultBytes);
            string pattern = @"->\|(?<Result>.*)\|<-";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                string tmp = m.Groups["Result"].Value;
                string err = MatchResultIsError(tmp);
                if (string.IsNullOrEmpty(err))
                {
                    return tmp;
                }
                else
                {
                    throw new ResponseCustomException("匹配到错误", err, result);
                }
            }
        }
        public static string MatchResultIsError(string result)
        {
            string pattern = @"^ERROR://(?<Error>.*)$";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                return null;
            }
            else
            {
                return m.Groups["Error"].Value;
            }
        }
        public static string MatchHexResultIsError(string result)
        {
            string pattern = @"^4552524F523A2F2F(?<Error>.*)$";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                return null;
            }
            else
            {
                return m.Groups["Error"].Value;
            }
        }

        public static bool MatchResultToBool(byte[] resultBytes, Encoding encode)
        {
            string strBool = GetResultFromInterval(resultBytes, encode);
            return strBool == "1" ? true : false;
        }
        public static byte[] MatchResultToFile(byte[] resultBytes, Encoding encode)
        {
            /**
             * 将byte数组先转化为十六进制字符串
             * 然后利用正则匹配出文件正文，->| |<- =>  2D3E7C 7C3C2D
             */
            string result = BitConverter.ToString(resultBytes).Replace("-",null);
            string pattern = @"2D3E7C(?<File>.*)7C3C2D";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                string strFile = m.Groups["File"].Value;
                string err = MatchHexResultIsError(strFile);
                if (string.IsNullOrEmpty(err))
                {
                    //十六进制的字符串转换为byte数组
                    return DataConvert.HexStrToBytes(strFile);
                }
                else
                {
                    byte[] errBytes = DataConvert.HexStrToBytes(err);
                    throw new ResponseCustomException("匹配到错误", encode.GetString(errBytes), result);
                }               
            }
        }
        public static string MatchResultToString(byte[] resultBytes, Encoding encode)
        {
            string content = GetResultFromInterval(resultBytes, encode);
            return content;
        }
        public static OsDisk MatchResultToOsDisk(byte[] resultBytes, Encoding encode)
        {
            string result = GetResultFromInterval(resultBytes, encode);
            string pattern = @"^(?<ShellDir>.*?)\t(?<AvailableDisk>.*?)$";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                OsDisk disk = new OsDisk();
                disk.ShellDir = m.Groups["ShellDir"].Value;
                disk.AvailableDisk = m.Groups["AvailableDisk"].Value;
                return disk;
            }
        }
        public static OsInfo MatchResultToOsInfo(byte[] resultBytes, Encoding encode)
        {
            string result = GetResultFromInterval(resultBytes, encode);
            string pattern = @"^(?<ShellDir>.*?)\t(?<Platform>.*?)\t(?<CurrentUser>.*?)\t(?<DirSeparators>.*?)$";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                OsInfo info = new OsInfo();
                info.ShellDir = m.Groups["ShellDir"].Value;
                info.Platform = m.Groups["Platform"].Value;
                info.CurrentUser = m.Groups["CurrentUser"].Value;
                info.DirSeparators = m.Groups["DirSeparators"].Value;
                return info;
            }
        }
        public static CmdResult MatchResultToCmdResult(byte[] resultBytes, Encoding encode)
        {
            string result = GetResultFromInterval(resultBytes, encode);
            string pattern = @"(?<cmd>.*?)\[S\][\r\n|\n]+(?<curdir>.*?)[\r\n|\n]+\[E\]";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                string cmd = m.Groups["cmd"].Value;
                string curdir = m.Groups["curdir"].Value;
                CmdResult cmdResult = new CmdResult();
                cmdResult.Result = cmd;
                cmdResult.CurrentDir = curdir;
                return cmdResult;
            }
        }
        public static List<OsFile> MatchResultToOsFile(byte[] resultBytes, Encoding encode)
        {
            string result = GetResultFromInterval(resultBytes, encode);
            //如果result为空，则说明此文件夹为空
            if (result == "")
            {
                return new List<OsFile>();
            }
            string pattern = @"(?<filename>.*?)\t(?<filemtime>.*?)\t(?<filesize>.*?)\t(?<fileperms>.*?)\n";
            Regex regex = new Regex(pattern, RegexOptions.Singleline);
            Match m = regex.Match(result);
            if (!m.Success)
            {
                throw new ResponseCustomException("匹配返回结果", "匹配失败", result);
            }
            else
            {
                List<OsFile> filetree = new List<OsFile>();
                while (m.Success)
                {
                    string filename = m.Groups["filename"].Value;
                    string filemtime = m.Groups["filemtime"].Value;
                    string filesize = m.Groups["filesize"].Value;
                    string fileperms = m.Groups["fileperms"].Value;
                    filetree.Add(new OsFile(filename, filemtime, filesize, fileperms));
                    m = m.NextMatch();
                }
                return filetree;
            }
        }
    }
}