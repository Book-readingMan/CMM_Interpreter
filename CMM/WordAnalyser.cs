﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CMM
{
    class WordAnalyser
    {
        //单词
        static List<(string, int)> tokens;
        //关键字和特殊符号
        static string[] keywords = { "if", "else", "while", "read", "write", "int", "real" };
        static string[] symbols = { "+", "-", "*", "/", "=", "<", ">", "==", "<>", "(", ")", ";", "{", "}", "/*", "*/", "[", "]", ",", "." };
        //用于单词编码
        static int sStart = keywords.Length;
        static int oStart = symbols.Length;

        static string buffer;
        static int index;
        static char ch;
        static string input;
        static int value;

        //读取下一个字符
        static char Peek()
        {
            ++index;
            try
            {
                ch = input[index];
            }
            //读完输入串以\0为标志
            catch (IndexOutOfRangeException)
            {
                ch = '\0';
            }
            return ch;
        }
        //加入缓冲
        static void Concat()
        {
            buffer += ch;
        }
        //回退
        static void Retract()
        {
            ch = input[--index];
        }
        //读取单词
        static void Read()
        {
            tokens.Add((buffer, value));
            buffer = "";
        }
        //分析特殊符号
        static void AnalyseSymbol()
        {

            switch (ch)
            {
                case '=':
                    if (Peek() == '=')
                        Concat();
                    break;
                case '<':
                    if (Peek() == '>')
                        Concat();
                    break;
                case '/':
                    if (Peek() != '*' && ch != '\0')
                    {
                        Retract();
                        break;
                    }
                    //跳过注释
                    buffer = "";
                    while ((Peek() != '*' || Peek() != '/') && ch != '\0') ;
                    return;
            }

            value = Array.IndexOf(symbols, buffer) + sStart;
            if (value == -1)
                throw new Exception("特殊符号错误");
            Read();

        }
        //分析数字
        static void AnalyseNumber()
        {
            for (Peek(); Char.IsDigit(ch) || ch == '.'; Peek())
            {
                if (Char.IsDigit(ch))
                {
                    Concat();
                    continue;
                }
                //小数点只有一个，且小数点后是数字
                if (buffer.Contains('.') || !Char.IsDigit(Peek()))
                    throw new Exception("小数点错误");
                if (ch != '\0')
                    Retract();
                Concat();
            }
            if (ch != '\0')
                Retract();
            value = oStart;
            if (buffer.Contains('.'))
                value = oStart + 1;
            Read();
        }
        //分析标识符和保留字
        static void AnalyseIdAndKey()
        {

            for (Peek(); Char.IsLetterOrDigit(ch) || ch == '_'; Peek())
            {
                Concat();
            }
            if (ch != '\0')
                Retract();
            value = Array.IndexOf(keywords, buffer);
            if (value == -1)
                value = oStart + 2;
            if (buffer.Last() == '_')
                throw new Exception("标识符错误");
            Read();

        }
        //词法分析
        public static List<(string, int)> Analyse(string inputStr)
        {
            if (inputStr == "")
                return null;
            input = inputStr;
            tokens = new List<(string, int)>();
            index = 0;
            buffer = "";
            ch = input[index];

            do
            {
                Concat();
                try
                {
                    if (Array.IndexOf(symbols, buffer) != -1)
                        AnalyseSymbol();
                    else if (Char.IsDigit(ch))
                        AnalyseNumber();
                    else if (Char.IsLetter(ch) || ch == '_')
                        AnalyseIdAndKey();
                    else if (Char.IsWhiteSpace(ch))
                        buffer = "";
                    else
                        throw new Exception("未知符号错误");
                }
                catch (Exception ex)
                {
                    buffer = ex.Message;
                    value = -1;
                    Read();
                }
                finally
                {
                    Peek();
                }

            } while (ch != '\0');

            return tokens;
        }

    }
}
