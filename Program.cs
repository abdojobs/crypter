/*
Copyright 2013 José A. Rojo L.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

using Jarol.Console;

namespace crypter
{
    class Program
    {
        private static bool  m_bl = true;

        //----------------------------------------------------------------------------------

        private const string SALT      = "PkZrST6";
        private const string IV        = "AgTxp96*Zf8e12Xy";
        private const string ALGORITHM = "SHA1";

        //----------------------------------------------------------------------------------

        private static byte[] EncodeToAes
        (
              byte[]   src
            , string   pwd
            , ref long length
            , string   salt      = Program.SALT                                              // Salt to encrypt with.
            , string   algorithm = Program.ALGORITHM                                         // SHA1 or MD5.
            , string   siv       = Program.IV                                                // Initial Vector Needs to be 16 ASCII characters long.
            , int      keysize   = 256                                                       // Can be 128, 192, or 256
            , int      ipwd      = 2                                                         // Number of iterations to do.
        ){
            byte[]              iv = Encoding.ASCII.GetBytes(siv);
            PasswordDeriveBytes pd = new PasswordDeriveBytes
            (
                  pwd
                , Encoding.ASCII.GetBytes(salt)
                , algorithm
                , ipwd
            );

            RijndaelManaged rm  = new RijndaelManaged();
            byte[]          rt  = null;

            rm.Mode = CipherMode.CBC;
            using (ICryptoTransform ct = rm.CreateEncryptor(pd.GetBytes(keysize / 8), iv))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Write))
                    {
                        for (long i = 0; i < length; ++i)
                            cs.WriteByte(src[i]);

                        cs.FlushFinalBlock();

                        length = ms.Length;
                        rt     = ms.ToArray();

                        ms.Close();
                        cs.Close();
                    }
                }
            }

            rm.Clear();
            return rt;
        }

        //----------------------------------------------------------------------------------

        private static byte[] DecodeFromAes
        (
              byte[]   src
            , string   pwd
            , ref long length 
            , string   salt      = Program.SALT
            , string   algorithm = Program.ALGORITHM
            , string   siv       = Program.IV
            , int      keysize   = 256
            , int      ipwd      = 2
        ){
            byte[]              iv = Encoding.ASCII.GetBytes(siv);
            PasswordDeriveBytes pd = new PasswordDeriveBytes
            (
                  pwd
                , Encoding.ASCII.GetBytes(salt)
                , algorithm
                , ipwd
            );

            RijndaelManaged rm = new RijndaelManaged();
            byte[]          bf = new byte[src.Length];

            length  = 0;
            rm.Mode = CipherMode.CBC;
            using (ICryptoTransform ct = rm.CreateDecryptor(pd.GetBytes(keysize / 8), iv))
            {
                using (MemoryStream ms = new MemoryStream(src))
                {
                    using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Read))
                    {
                        for (int bt; (bt = cs.ReadByte()) != -1; ++length)
                            bf[length] = (byte)bt;

                        ms.Close();
                        cs.Close();
                    }
                }
            }

            rm.Clear();
            return bf;
        }

        //----------------------------------------------------------------------------------

        private static void ShowLogo()
        {
            if (m_bl)
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                Messenger.Print
                (
                      Messenger.Frame.SIMPLE
                    , "Crypter Tool v." + v.Major + "." + v.Minor + "." +
                      v.Build + "." + v.Revision + "\n" +
                      "Copyright (c) 2013 José A. Rojo L."
                    , ConsoleColor.DarkGreen
                    , ConsoleColor.DarkGreen
                    , true
                );

                m_bl = false;
            }
        }

        //----------------------------------------------------------------------------------

        private static void ShowHelp()
        {
            Program.ShowLogo();
            Messenger.Print
            (
                  "\n Usage:\n\n\t\rcrypter [options...] input-file-name\n"                           +
                  "\n \rOptions:\n\n"                                                                 +
                  "\t\r-c  --codec           \rb64 (Base 64) or AES (Advanced Encryption"             +
                  "\n\t\t\t      Standard).\n\n"                                                      +
                  "\t\r-e  --encode          \rEncode operation indicator.\n"                         +
                  "\t\r-d  --decode          \rDecode operation indicator.\n\n"                       +
                  "\t\r-k  --key-size        \rFor AES only. 128, 192, or 256 (By default).\n"        +
                  "\t\r-p  --password        \rFor AES only. Word or phrase. Required parameter.\n"   +
                  "\t\r-s  --salt            \rFor AES only. By default: \"" + Program.SALT + "\".\n" +
                  "\t\r-a  --algorithm       \rFor AES only. MD5 or SHA1 (By default).\n"             +
                  "\t\r-i  --initial-vector  \rFor AES only. Needs to be 16 ASCII characte"           +
                  "rs\n\t\t\t      long (By default: \"" + Program.IV + "\").\n\n"                    +
                  "\t\r-o  --output-file     \rOutput file name.\n"                                   +
                  "\t\r-w  --overwrite       \rOverwrites the existing output file(s) without"        +
                  "\n\t\t\t      asking.\n\n"                                                         +
                  "\t\r-h  --help            \rShow this screen.\n"                                   +
                  "\n \rSamples:\n\n"                                                                 +
                  "\t\r<1>\r crypter -o myfile.b64 -c b64 -e myfile.txt\n"                            +
                  "\t\r<2>\r crypter -o myfile.txt -c b64 -d myfile.b64\n"                            +
                  "\t\r<3>\r crypter -o myfile.aes -c aes -p \"my password\" -e myfile.bin\n"         +
                  "\t\r<4>\r crypter -o myfile.bin -c aes -p \"my password\" -d myfile.aes\n"         +
                  "\t\r<5>\r crypter -o myfile.aes -c aes -e myfile.bin\n"

                , new ConsoleColor[]
                  {
                        ConsoleColor.Yellow
                      , ConsoleColor.Gray

                      , ConsoleColor.Yellow

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.Yellow

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray

                      , ConsoleColor.White
                      , ConsoleColor.Gray
                      
                      , ConsoleColor.White
                      , ConsoleColor.Gray
                  }
            );
        }

        //----------------------------------------------------------------------------------

        static void Main (string[] args)
        {
            string salt      = Program.SALT;
            string algorithm = Program.ALGORITHM;
            string iv        = Program.IV;
            string pwd       = string.Empty;
            string codec     = string.Empty;
            string ifn       = string.Empty;
            string ofn       = string.Empty;
            byte   op        = 0;
            int    iks       = 256;
            bool   bow       = false;

            int l = args.Length;

            if (l < 1)
                Program.ShowHelp();

            else
            {
                try
                {
                    for (int i = 0; i < l; ++i)
                    {
                        switch (args[i].ToLower())
                        {
                            case "-e":
                            case "--encode":
                                op = 1;
                                break;

                            case "-d":
                            case "--decode":
                                op = 2;
                                break;

                            case "-o":
                            case "--output-file":
                                ofn = args[++i];
                                break;

                            case "-w":
                            case "--overwrite":
                                bow = true;
                                break;

                            case "-k":
                            case "--key-size":
                                switch (iks = Convert.ToInt32(args[++i]))
                                {
                                    case 128:
                                    case 192:
                                    case 256:
                                        break;

                                    default:
                                        throw new Exception("Invalid key size!");
                                }
                                break;

                            case "-p":
                            case "--password":
                                pwd = args[++i];
                                break;

                            case "-c":
                            case "--codec":
                                codec = args[++i].ToLower();
                                break;

                            case "-a":
                            case "--algorithm":
                                switch (algorithm = args[++i].ToUpper())
                                {
                                    case Program.ALGORITHM:
                                    case "MD5":
                                        break;

                                    default:
                                        throw new Exception("Invalid algorithm!");
                                }
                                break;

                            case "-s":
                            case "--salt":
                                salt = args[++i];
                                break;

                            case "-i":
                            case "--initial-vector":
                                if (args[++i].Length == 16)
                                    iv = args[i];

                                else throw new Exception
                                (
                                    "Initial Vector does not have 16 characters!"
                                );
                                break;

                            case "-h":
                            case "--help":
                                Program.ShowHelp();
                                Environment.Exit(0);
                                break;

                            default:
                                ifn = args[i];
                                break;
                        }
                    }

                    Program.ShowLogo();

                    if (File.Exists(ifn))
                    {
                        BinaryReader br = new BinaryReader(File.Open(ifn, FileMode.Open));
                        long         ln = br.BaseStream.Length;
                        byte[]       bf = new byte[ln];

                        for (long i = 0; i < ln; ++i)
                            bf[i] = br.ReadByte();

                        br.Close();

                        Messenger.Print
                        (
                              Messenger.Icon.INFORMATION
                            , String.Format
                              (
                                    "{0}, please wait... "
                                  , op == 1 ? "Encoding" : "Decoding"
                              )
                        );

                        switch (codec)
                        {
                            case "b64":
                                if (op == 1) bf = ASCIIEncoding.ASCII.GetBytes
                                (
                                    Convert.ToBase64String(bf)
                                );

                                else if (op == 2) bf = Convert.FromBase64String
                                (
                                    ASCIIEncoding.ASCII.GetString(bf)
                                );

                                ln = bf.Length;
                                break;

                            case "aes":
                                while (pwd.Length == 0)
                                {
                                    Messenger.Print(Messenger.Icon.WARNING, "Password: ");

                                    do
                                    {
                                        ConsoleKeyInfo ki = Console.ReadKey(true);

                                        if (ki.Key == ConsoleKey.Enter)
                                            break;

                                        if (ki.Key == ConsoleKey.Backspace)
                                        {
                                            if (pwd.Length > 0)
                                            {
                                                pwd = pwd.Remove(pwd.Length - 1);
                                                Console.SetCursorPosition
                                                (
                                                      Console.CursorLeft - 1
                                                    , Console.CursorTop
                                                );

                                                Console.Write(' ');
                                                Console.SetCursorPosition
                                                (
                                                      Console.CursorLeft - 1
                                                    , Console.CursorTop
                                                );
                                            }
                                        }

                                        else if 
                                        (
                                            ((byte)ki.KeyChar > 31 && (byte)ki.KeyChar < 127) || 
                                            ((byte)ki.KeyChar > 128)
                                        ){
                                            pwd += ki.KeyChar;
                                            Console.Write('*');
                                        }
                                    }
                                    while (true);
                                }

                                if (op == 1)
                                    bf = Program.EncodeToAes(bf, pwd, ref ln, salt, algorithm, iv, iks);

                                else if (op == 2)
                                    bf = Program.DecodeFromAes(bf, pwd, ref ln, salt, algorithm, iv, iks);

                                break;

                            default:
                                throw new Exception("Codec no found!");
                        }

                        if (ofn.Length > 0)
                        {
                            if (File.Exists(ofn))
                            {
                                if (!bow)
                                {
                                    bow = ConsoleKey.Y == Messenger.Print
                                    (
                                          Messenger.Icon.QUESTION
                                        , String.Format
                                            (
                                                "The file \"{0}\"\n" +
                                                "     Already exists! Overwrite?"
                                            , Path.GetFullPath(ofn)
                                            )
                                        , new ConsoleKey[] { ConsoleKey.Y, ConsoleKey.N }
                                        , true
                                        , true
                                    );
                                }

                                if (bow)
                                    File.Delete(ofn);

                                else
                                {
                                    Messenger.Print
                                    (
                                          Messenger.Icon.WARNING
                                        , "Process canceled by user!\n"
                                    );

                                    Environment.Exit(0);
                                }
                            }

                            BinaryWriter bw = new BinaryWriter(File.Open(ofn, FileMode.CreateNew));
                            
                            for (long i = 0; i < ln; ++i)
                                bw.Write(bf[i]);

                            bw.Close();

                            Messenger.Print(Messenger.Icon.INFORMATION, "Done!\n");
                        }
                    }

                    else
                    {
                        throw new Exception
                        (
                            String.Format
                            (
                                  "File \"{0}\" no found!"
                                , ifn
                            )
                        );
                    }
                }

                catch (Exception e)
                {
                    Program.ShowLogo();

                    if (e.Message.Length > 0)
                        Messenger.Print(Messenger.Icon.ERROR, e.Message, false, true);

                    Environment.Exit(1);
                }
            }
        }
    }
}
