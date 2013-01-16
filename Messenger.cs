/*
Copyright 2012 José A. Rojo L.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License
*/

using System;
using System.Text;
using System.Media;

using System.Text.RegularExpressions;

//---------------------------------------------------------------------------------

namespace Jarol.Console
{
    class Messenger
    {
        //-------------------------------------------------------------------------

        public static char ColorChar = '\r';

        //-------------------------------------------------------------------------

        public enum Icon
        {
              INFORMATION
            , QUESTION
            , WARNING
            , ERROR
            , UNKNOWN
        };

        //-------------------------------------------------------------------------

        public enum Frame
        {
              SIMPLE
            , DOUBLE
            , EXTRA
        };

        //-------------------------------------------------------------------------
        
        /*
        public enum Table
        {
            SIMPLE
          , DOUBLE
          , EXTRA
        };
        */

        //-------------------------------------------------------------------------

        public static void Print
        (
              string         msg											        // Message.
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
        ){
            string[] s = msg.Split(Messenger.ColorChar);

            for (int i = 0; i < s.Length; ++i)
            {
                if (fclr != null && i < fclr.Length)
                    System.Console.ForegroundColor = fclr[i];

                if (bclr != null && i < bclr.Length)
                    System.Console.BackgroundColor = bclr[i];

                System.Console.Write(s[i]);
            }

            System.Console.ResetColor();
        }

        //-------------------------------------------------------------------------

        public static void Print (string msg, ConsoleColor[] fclr)
        {
            Messenger.Print(msg, fclr, null);
        }

        //-------------------------------------------------------------------------

        public static void Print
        (
              string       msg                                                      // Message.
            , ConsoleColor fclr												        // Foreground colors.
            , ConsoleColor bclr												        // Background colors.
        ){
            Messenger.Print
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
            );
        }

        //-------------------------------------------------------------------------

        public static void Print
        (
              string       msg                                                      // Message.
            , ConsoleColor fclr												        // Foreground colors.
        ){
            Messenger.Print(msg, new ConsoleColor[] { fclr });
        }

        //-------------------------------------------------------------------------

        public static ConsoleKey Print
        (
              Icon           icn
            , string         msg                                                    // Message.
//          , ConsoleColor[] fclrs                                                  // Foreground colors. 
            , ConsoleKey[]   cks                                                    // Response keys.
//          , bool           bhk												    // Hide response keys.
            , bool           bss	    											// System sounds ('\a').
            , bool           bnl												    // New line.
        ){
            StringBuilder  sb = new StringBuilder("\n[");
            ConsoleKeyInfo ki = new ConsoleKeyInfo();
            ConsoleColor[] cc;

            switch (icn)
            {
                case Icon.INFORMATION:
                    if (bss)
                        SystemSounds.Beep.Play();                                   //SystemSounds.Asterisk.Play();

                    sb.Append('i');

                    cc = new ConsoleColor[] 
                    {
                          ConsoleColor.DarkGreen
                        , ConsoleColor.Gray
                    };
                    break;

                case Icon.QUESTION:
                    if (bss)
                        SystemSounds.Beep.Play();                                   //SystemSounds.Question.Play();

                    sb.Append('?');

                    cc = new ConsoleColor[] 
                    {
                          ConsoleColor.DarkCyan
                        , ConsoleColor.White
                    };
                    break;

                case Icon.WARNING:
                    if (bss)
                        SystemSounds.Beep.Play();                                   //SystemSounds.Exclamation.Play();

                    sb.Append('!');

                    cc = new ConsoleColor[] 
                    {
                          ConsoleColor.Yellow
                        , ConsoleColor.White
                    };
                    break;

                case Icon.ERROR:
                    if (bss)
                        SystemSounds.Beep.Play();                                   //SystemSounds.Hand.Play();

                    sb.Append('x');

                    cc = new ConsoleColor[] 
                    {
                          ConsoleColor.Red
                        , ConsoleColor.Yellow
                    };
                    break;

                default:
                    if (bss)
                        SystemSounds.Beep.Play();

                    sb.Append('-');

                    cc = new ConsoleColor[] 
                    {
                          ConsoleColor.DarkYellow
                        , ConsoleColor.DarkGray
                    };
                    break;
            }

            sb.Append("]:");
            sb.Append(Messenger.ColorChar);
            sb.Append(' ');
            sb.Append(msg);

            Messenger.Print(sb.ToString(), cc);

            if (cks != null && cks.Length > 0)
            {
                cc = new ConsoleColor[(cks.Length * 2) + 1];
                sb = new StringBuilder(" [");
                
                cc[0] = ConsoleColor.Gray;

                for (int i = 0, n = cks.Length - 1, f = 1; i < cks.Length; i++)
                {
                    sb.Append(Messenger.ColorChar);
                    sb.Append(cks[i].ToString());
                    sb.Append(Messenger.ColorChar);
                    sb.Append(i != n ? ", " : "]:");

                    cc[f++] = ConsoleColor.Yellow;
                    cc[f++] = ConsoleColor.DarkGray;
                }

                cc[cc.Length - 1] = ConsoleColor.Gray;
                Messenger.Print(sb.ToString(), cc);

                bool b = false;

                do
                {
                    ki = System.Console.ReadKey(true);

                    for (int i = 0; i < cks.Length; i++)
                        if ((b = ki.Key == cks[i]))
                            break;
                }
                while (!b);
            }

            if (bnl)
                System.Console.WriteLine();

            return ki.Key;
        }

        //-------------------------------------------------------------------------

        public static ConsoleKey Print 
        (
              Icon         icn
            , string       msg
            , ConsoleKey[] cks
            , bool         bss
        ){
            return Messenger.Print(icn, msg, cks, bss, false);
        }

        //-------------------------------------------------------------------------

        public static void Print (Icon icn, string msg, bool bss, bool bnl)
        {
            Messenger.Print(icn, msg, null, bss, bnl);
        }

        //-------------------------------------------------------------------------

        public static void Print (Icon icn, string msg, bool bss)
        {
            Messenger.Print(icn, msg, null, bss, false);
        }

        //-------------------------------------------------------------------------

        public static void Print (Icon icn, string msg)
        {
            Messenger.Print(icn, msg, null, false, false);
        }

        //-------------------------------------------------------------------------

        private static void Print
        (
              char[]       chrs                                                  // 
            , string       msg
            , ConsoleColor fclr                                                  // Frame color.
            , ConsoleColor tclr                                                  // Text color.
            , bool         bcwa                                                  // Console width adjust.
        ){
            int l = 0;

            if (chrs.Length > 0)
                while (chrs.Length < 7)
                    chrs.SetValue(l++, chrs[0]);

            if (chrs.Length > 6 && msg.Length > 0)
            {
                StringBuilder s = new StringBuilder(msg);
                l = 77; //l = System.Console.BufferWidth - 3;                    // linux?

                for (int i = 0, n = l - 2, x, y = 0; i < s.Length; ++i)
                {
                    if (s[i] == '\b')
                        s.Remove(--i, 2);

                    if (s[i] == '\n')
                        n += ++i;

                    if (s[i] == '\t')
                    {
                        s.Remove(i, 1);

                        x  = 8;
                        x -= i - (++y * x) + x;

                        while (x-- > 0)
                            s.Insert(i, ' ');
                    }

                    if (i >= n)
                    {
                        s.Insert(i, '\n');
                        n += ++i;
                    }
                }

                msg = s.ToString();
                s   = new StringBuilder();

                string[]       m = msg.Split(new char[] { '\n', '\r' });
                ConsoleColor[] a = new ConsoleColor[(m.Length * 2) + 1];

                if (!bcwa)
                    l = m[0].Length + 2;

                for (int i = 0; i < m.Length; ++i)
                {
                    if (l < m[i].Length)
                        l = m[i].Length + 2;

                    m[i] = ' ' + m[i];
                }

                a[0] = fclr;
                s.Append(chrs[0]);
                
                for (int i = 0; i < l; ++i)
                    s.Append(chrs[1]);

                s.Append(chrs[2]);

                for (int i = 0, n = 1; i < m.Length; ++i)
                {
                    s.Append('\n');
                    s.Append(chrs[3]);
                    s.Append(Messenger.ColorChar);
                    s.Append(m[i]);
                    
                    for (int j = m[i].Length; j < l; ++j)
                        s.Append(' ');

                    s.Append(Messenger.ColorChar);
                    s.Append(chrs[3]);

                    a[n++] = tclr;
                    a[n++] = fclr;
                }

                s.Append('\n');
                s.Append(chrs[4]);

                for (int i = 0; i < l; ++i)
                    s.Append(chrs[5]);

                s.Append(chrs[6]);
                s.Append('\n');

                Messenger.Print(s.ToString(), a); 
            }
        }

        //-------------------------------------------------------------------------

        public static void Print
        (
              Frame        frm
            , string       msg
            , ConsoleColor fclr                                                     // Frame color.
            , ConsoleColor tclr                                                     // Text color.
            , bool         bcwa                                                     // Console width adjust.
        ){
            if (frm == Frame.DOUBLE)
            {
                Messenger.Print
                (
                      new char[] { '╔', '═', '╗', '║', '╚', '═', '╝' }
                    , msg
                    , fclr
                    , tclr
                    , bcwa
                );
            }

            else if (frm == Frame.EXTRA)
            {
                Messenger.Print
                (
                      new char[] { '█', '▀', '█', '█', '█', '▄', '█' }
                    , msg
                    , fclr
                    , tclr
                    , bcwa
                );
            }

            else Messenger.Print
            (
                  new char[] { '┌', '─', '┐', '│', '└', '─', '┘' }
                , msg
                , fclr
                , tclr
                , bcwa
            );
        }

        //-------------------------------------------------------------------------

        public static void Print
        (
              Frame        frm
            , string       msg
            , ConsoleColor fclr                                                     // Frame and text color.
            , bool         bcwa                                                     // Console width adjust.
        ){
            Messenger.Print(frm, msg, fclr, fclr, bcwa);
        }

        //-------------------------------------------------------------------------

        public static void Print (Frame frm, string msg, ConsoleColor fclr)
        {
            Messenger.Print(frm, msg, fclr, true);
        }

        //-------------------------------------------------------------------------

        public static void Print
        (
              char         cfrm                                                     // Frame char.
            , string       msg
            , ConsoleColor fclr                                                     // Frame color.
            , ConsoleColor tclr                                                     // Text color.
            , bool         bcwa                                                     // Console width adjust.
        ){

            Messenger.Print
            (
                  new char[] { cfrm, cfrm, cfrm, cfrm, cfrm, cfrm, cfrm }
                , msg
                , fclr
                , tclr
                , bcwa
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object[]       args
        ){

            Regex rx = new Regex
            (
                  @"\{\d+(\:[A-Za-z]\}|\})"
                , RegexOptions.Multiline
            );

            MatchCollection mc = rx.Matches(msg);

            for (int i = 0, l = mc.Count; i < l; ++i)
            {
                Capture       c = mc[i].Captures[0];
                int           n = int.Parse(c.Value[1].ToString());
                StringBuilder s;

                if (c.Value.IndexOf('m') != -1)
                {
                    s = new StringBuilder(args[n].ToString());

                    for (int j = s.Length, p = j - 3; j > 0; --j)
                    {
                        if (j == p)
                        {
                            s.Insert(j, '.');
                            p -= 3;
                        }
                    }

                    msg = msg.Replace(c.Value, s.ToString());
                }

                if (c.Value.IndexOf('p') != -1)
                {
                    double d = double.Parse(args[n].ToString());

                    s   = new StringBuilder(Math.Round(d, 2).ToString());
                    msg = msg.Replace(c.Value, s.Append('%').ToString());
                }
            }

            Messenger.Print(String.Format(msg, args), fclr, bclr);
        }
        
        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object         arg0
        ){
            Messenger.Printf(msg, fclr, bclr, new object[] { arg0 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										           // Foreground colors.
            , ConsoleColor[] bclr											       // Background colors.
            , object         arg0
            , object         arg1
        ){
            Messenger.Printf(msg, fclr, bclr, new object[] { arg0, arg1 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
        ){
            Messenger.Printf (msg, fclr, bclr, new object[] { arg0, arg1, arg2 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										           // Foreground colors.
            , ConsoleColor[] bclr											       // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , bclr
                , new object[] { arg0, arg1, arg2, arg3 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										           // Foreground colors.
            , ConsoleColor[] bclr											       // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , bclr
                , new object[] { arg0, arg1, arg2, arg3, arg4 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , bclr
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
            , object         arg6
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , bclr
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string         msg
            , ConsoleColor[] fclr										            // Foreground colors.
            , ConsoleColor[] bclr											        // Background colors.
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
            , object         arg6
            , object         arg7
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , bclr
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg7 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, ConsoleColor[] fclr, object[] args)
        {
            Messenger.Printf(msg, fclr, null, args);
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, ConsoleColor[] fclr, object arg0)
        {
            Messenger.Printf(msg, fclr, null, new object[] { arg0 });
        }

        //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr												    // Foreground colors.
            , object         arg0
            , object         arg1
        ){
            Messenger.Printf(msg, fclr, null, new object[] { arg0, arg1 });
        }

        //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
        ){
            Messenger.Printf(msg, fclr, null, new object[] { arg0, arg1, arg2 });
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , null
                , new object[] { arg0, arg1, arg2, arg3 }
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , null
                , new object[] { arg0, arg1, arg2, arg3, arg4 }
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , null
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5 }
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
            , object         arg6
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , null
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 }
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string         msg
            , ConsoleColor[] fclr
            , object         arg0
            , object         arg1
            , object         arg2
            , object         arg3
            , object         arg4
            , object         arg5
            , object         arg6
            , object         arg7
        ){
            Messenger.Printf
            (
                  msg
                , fclr
                , null
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, ConsoleColor fclr, object[] args)
        {
            Messenger.Printf(msg, new ConsoleColor[] { fclr }, null, args);
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, ConsoleColor fclr, object arg0)
        {
            Messenger.Printf(msg, new ConsoleColor[] { fclr }, arg0);
        }

        //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
        ){
            Messenger.Printf(msg, new ConsoleColor[] { fclr }, arg0, arg1);
        }

        //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
                , arg3
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
            , object       arg6
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
                , arg6
            );
        }

       //-------------------------------------------------------------------------

        public static void Printf 
        (
              string       msg
            , ConsoleColor fclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
            , object       arg6
            , object       arg7
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
                , arg6
                , arg7
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object[]     args
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , args
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
                , arg3
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
            , object       arg6
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
                , arg6
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string       msg
            , ConsoleColor fclr
            , ConsoleColor bclr
            , object       arg0
            , object       arg1
            , object       arg2
            , object       arg3
            , object       arg4
            , object       arg5
            , object       arg6
            , object       arg7
        ){
            Messenger.Printf
            (
                  msg
                , new ConsoleColor[] { fclr }
                , new ConsoleColor[] { bclr }
                , arg0
                , arg1
                , arg2
                , arg3
                , arg4
                , arg5
                , arg6
                , arg7
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, object[] args)
        {
            Messenger.Printf(msg, null, args);
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, object arg0)
        {
            Messenger.Printf(msg, new object[] { arg0 });
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, object arg0, object arg1)
        {
            Messenger.Printf(msg, new object[] { arg0, arg1 });
        }

        //-------------------------------------------------------------------------

        public static void Printf (string msg, object arg0, object arg1, object arg2)
        {
            Messenger.Printf(msg, new object[] { arg0, arg1, arg2 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string msg
            , object arg0
            , object arg1
            , object arg2
            , object arg3
        ){
            Messenger.Printf(msg, new object[] { arg0, arg1, arg2, arg3 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string msg
            , object arg0
            , object arg1
            , object arg2
            , object arg3
            , object arg4
        ){
            Messenger.Printf(msg, new object[] { arg0, arg1, arg2, arg3, arg4 });
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string msg
            , object arg0
            , object arg1
            , object arg2
            , object arg3
            , object arg4
            , object arg5
        ){
            Messenger.Printf
            (
                  msg
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string msg
            , object arg0
            , object arg1
            , object arg2
            , object arg3
            , object arg4
            , object arg5
            , object arg6
        ){
            Messenger.Printf
            (
                  msg
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6 }
            );
        }

        //-------------------------------------------------------------------------

        public static void Printf
        (
              string msg
            , object arg0
            , object arg1
            , object arg2
            , object arg3
            , object arg4
            , object arg5
            , object arg6
            , object arg7
        ){
            Messenger.Printf
            (
                  msg
                , new object[] { arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7 }
            );
        }
    }
}
