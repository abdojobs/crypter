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

        private const string  SALT = "PkZrST68";
        private const string  IV   = "AgTxp96*Zf8e12Xy";
        private const string  HASH = "SHA256";
        private const int     INUM = 2;

        //--------------------------------------------------------------------------------
        // PBKDF2Sha256GetBytes
        //--------------------------------------------------------------------------------
        // Author: 
        // Peter O.
        //--------------------------------------------------------------------------------
        // Refs: 
        // http://upokecenter.dreamhosters.com/articles/2012/02/net-pbkdf2-sha256/
        // http://stackoverflow.com/questions/18648084/rfc2898-pbkdf2-with-sha256-as-digest-in-c-sharp
        //--------------------------------------------------------------------------------

        public static byte[] PBKDF2Sha256GetBytes
        (
              int    dklen
            , byte[] password
            , byte[] salt
            , int    iterationCount
        ){

            using (var hmac = new HMACSHA256(password))
            {
                int hashLength = hmac.HashSize / 8;

                if ((hmac.HashSize & 7) != 0)
                    hashLength++;

                int keyLength = dklen / hashLength;

                /*
                if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                    throw new ArgumentOutOfRangeException("dklen");
                */
                
                if (dklen % hashLength != 0)
                    keyLength++;
            
                byte[] extendedkey = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);

                using (var ms = new System.IO.MemoryStream())
                {
                    for (int i = 0; i < keyLength; i++)
                    {
                        extendedkey[salt.Length]     = (byte)(((i + 1) >> 24) & 0xFF);
                        extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                        extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                        extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);

                        byte[] u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, salt.Length, 4);
                        byte[] f = u;

                        for (int j = 1; j < iterationCount; j++)
                        {
                            u = hmac.ComputeHash(u);

                            for (int k = 0; k < f.Length; k++)
                                f[k] ^= u[k];
                        }

                        ms.Write(f, 0, f.Length);
                        Array.Clear(u, 0, u.Length);
                        Array.Clear(f, 0, f.Length);
                    }

                    byte[] dk = new byte[dklen];
                    ms.Position = 0;
                    ms.Read(dk, 0, dklen);
                    ms.Position = 0;

                    for (long i = 0; i < ms.Length; i++)
                        ms.WriteByte(0);

                    Array.Clear(extendedkey, 0, extendedkey.Length);
                    return dk;
                }
            }
        }

        //--------------------------------------------------------------------------------


        private static byte[] KeyGen
        (
              string pwd                                                                     // Password. Word or phrase.
            , string salt                                                                    // Salt to encrypt with.
            , string hash                                                                    // MD5, SHA1, or SHA256.
            , int    keysize                                                                 // Can be 128, 192, or 256
            , int    inum                                                                    // Number of iterations to do.
        ){
            switch ((hash = hash.ToUpper()))
            {
                case "SHA256":
                    /*
                    Rfc2898DeriveBytes rd = new Rfc2898DeriveBytes
                    (
                          pwd
                        , Encoding.ASCII.GetBytes(salt)
                        , inum
                    );
                    

                    return rd.GetBytes(keysize / 8);
                    */
                    
                    return PBKDF2Sha256GetBytes
                    (
                          keysize / 8
                        , Encoding.ASCII.GetBytes(pwd)
                        , Encoding.ASCII.GetBytes(salt)
                        , inum
                    );

                default:
                    PasswordDeriveBytes pd = new PasswordDeriveBytes
                    (
                          pwd
                        , Encoding.ASCII.GetBytes(salt)
                        , hash
                        , inum
                    );

                    return pd.GetBytes(keysize / 8);
            }
        }

        //----------------------------------------------------------------------------------

        private static byte[] Encrypt
        (
              byte[]     src                                                                   // Source bytes. 
            , string     pwd                                                                   // Password.
            , ref long   length                                                                // Source long.
            , string     salt
            , string     hash                                         
            , string     siv                                                                   // Initial Vector Needs to be 16 ASCII characters long.
            , CipherMode mode
            , int        keysize                                                      
            , int        inum                                       
        ){
            RijndaelManaged rm = new RijndaelManaged();
            byte[]          kg = Program.KeyGen(pwd, salt, hash, keysize, inum);
            byte[]          iv = Encoding.ASCII.GetBytes(siv);
            byte[]          rt = null;

            rm.Mode    = mode;
            rm.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform ct = rm.CreateEncryptor(kg, iv))
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

        private static byte[] Decrypt
        (
              byte[]     src
            , string     pwd
            , ref long   length 
            , string     salt
            , string     hash
            , string     siv
            , CipherMode mode
            , int        keysize
            , int        inum
        ){
            RijndaelManaged rm = new RijndaelManaged();
            byte[]          kg = Program.KeyGen(pwd, salt, hash, keysize, inum);
            byte[]          iv = Encoding.ASCII.GetBytes(siv);
            byte[]          bf = new byte[length];

            length  = 0;
            rm.Mode = mode;

            using (ICryptoTransform ct = rm.CreateDecryptor(kg, iv))
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
                  "\t\r-m  --mode            \rb64 (Base 64) or AES (Advanced Encryption"             +
                  "\n\t\t\t      Standard).\n\n"                                                      +
                  "\t\r-e  --encrypt-encode  \rEncrypt or encode operation indicator.\n"              +
                  "\t\r-d  --decrypt-decode  \rDecrypt or decode operation indicator.\n\n"            +
                  "\t\r-k  --key-size        \rFor AES only. 128, 192, or 256 (By default).\n"        +
                  "\t\r-p  --password        \rFor AES only. Word or phrase. Required parameter.\n"   +
                  "\t\r-s  --salt            \rFor AES only. At least 8 characters for SHA256"        +
                  "\n\t\t\t      (By default: \"" + Program.SALT + "\").\n\n"                         +
                  "\t\r-h  --hash            \rFor AES only. MD5, SHA1 or SHA256 (By default).\n"     +
                  "\t\r-c  --cipher-mode     \rFor AES only. ECB or CBC (By default).\n" +
                  "\t\r-i  --initial-vector  \rFor AES only. Needs to be 16 ASCII characte"           +
                  "rs\n\t\t\t      long (By default: \"" + Program.IV + "\").\n\n"                    +
                  "\t\r-n  --num-iterations  \rNumber of iterations to do. Range from 1 to "          +
                  "\n\t\t\t      " + int.MaxValue.ToString() + " (By default: " + Program.INUM        + 
                  ").\n\n"                                                                            +
                  "\t\r-o  --output-file     \rOutput file name.\n"                                   +
                  "\t\r-w  --overwrite       \rOverwrites the existing output file(s) without"        +
                  "\n\t\t\t      asking.\n\n"                                                         +
                  "\t\r--help                \rShow this screen.\n"                                   +
                  "\n \rSamples:\n\n"                                                                 +
                  "\t\r<1>\r crypter -o myfile.b64 -m b64 -e myfile.txt\n"                            +
                  "\t\r<2>\r crypter -o myfile.txt -m b64 -d myfile.b64\n"                            +
                  "\t\r<3>\r crypter -o myfile.aes -m aes -p \"my password\" -e myfile.bin\n"         +
                  "\t\r<4>\r crypter -o myfile.bin -m aes -p \"my password\" -d myfile.aes\n"         +
                  "\t\r<5>\r crypter -o myfile.aes -m aes -e myfile.bin\n"

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
            CipherMode cm   = CipherMode.CBC;
            string     salt = Program.SALT;
            string     hash = Program.HASH;
            string     iv   = Program.IV;
            string     pwd  = string.Empty;
            string     mode = string.Empty;
            string     ifn  = string.Empty;
            string     ofn  = string.Empty;
            byte       op   = 0;
            int        iks  = 256;
            int        inum = Program.INUM; 
            bool       bow  = false;

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
                            case "--encrypt-encode":
                                op = 1;
                                break;

                            case "-d":
                            case "--decrypt-decode":
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

                            case "-m":
                            case "--mode":
                                mode = args[++i].ToLower();
                                break;

                            case "-h":
                            case "--hash":
                                switch (hash = args[++i].ToUpper())
                                {
                                    case "MD5":
                                    case "SHA1":
                                    case "SHA256":
                                        break;

                                    default:
                                        throw new Exception("Invalid algorithm!");
                                }
                                break;

                            case "-c":
                            case "--cipher-mode":
                                switch (args[++i].ToUpper())
                                {
                                    case "CBC":
                                        cm = CipherMode.CBC;
                                        break;

                                    case "ECB":
                                        cm = CipherMode.ECB;
                                        break;

                                    default:
                                        throw new Exception("Invalid Cipher-Mode!");
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

                            case "-n":  
                            case "--num-iterations":
                                if ((inum = Convert.ToInt32(args[++i])) < 1)
                                    inum = 1;
                                break;

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
                        Messenger.Print(Messenger.Icon.INFORMATION, "Processing. Please wait...");

                        switch (mode)
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
                                    bf = Program.Encrypt(bf, pwd, ref ln, salt, hash, iv, cm, iks, inum);

                                else if (op == 2)
                                    bf = Program.Decrypt(bf, pwd, ref ln, salt, hash, iv, cm, iks, inum);

                                break;

                            default:
                                throw new Exception("Mode no found!");
                        }

                        if (ofn.Length < 1)
                            Messenger.Print(Messenger.Icon.ERROR, "Output file name no found!\n");

                        else
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
