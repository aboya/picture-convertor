using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RPQ
{
    class PathCheck
    {
        static void Reverse(ref string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            s = new string(arr);
        }
        static void  FindBrackets(ref string name, out int a, out int b, int rightIndex)
	    {
		    int i;
            a = -1;
            b = -1;
		    if(name[rightIndex] != ')') return;
		    b = rightIndex;
		    for(i = rightIndex; i >= 0; i--)
		    {
			    if(a == -1 && name[i] == '(') a = i;
		    }
		    for(i = a + 1; i < b; i++)
			    if(!Char.IsDigit(name[i])) a = -1;
		    if(a + 1 >= b) a = -1;
	    }
        static int GetValue(ref string name, int a, int b)
	    {
		    string val = name.Substring(a + 1, b - a - 1);
		    if(val[0] == '0') return 0;
            return Convert.ToInt32(val);
	    }
        static void SetValue(ref string name, int a, int b, int val)
	    {
		    name = name.Remove(a + 1, b - a - 1);
            name = name.Insert(a + 1, val.ToString());
	    }
        public static void CheckExistFile(ref string path)
        {
            if (!File.Exists(path)) return;
            int i, n = path.Length, val = 1;
            string name = "";
            for (i = n - 1; i >= 0; i--)
                if (path[i] == '\\') break;
                else
                    name += path[i];

            path = path.Remove(i + 1);
            PathCheck.Reverse(ref name);

            n = name.Length;
            for (i = n - 1; i >= 0; i--)
                if (name[i] == '.') break;
            if (i == -1)
            {
                int a, b;
                PathCheck.FindBrackets(ref name, out a, out b, name.Length);
                if (a == -1) name += "(1)";
                else
                {
                    val = PathCheck.GetValue(ref name, a, b) + 1;
                    PathCheck.SetValue(ref name, a, b, val);
                }
                if (val > 100000) name = name.Insert(n - 1, "Copy (1)");
            }
            else
            {
                int a, b;
                PathCheck.FindBrackets(ref name, out a, out b, i - 1);
                if (a == -1) name = name.Insert(i, "(1)");
                else
                {
                    val = PathCheck.GetValue(ref name, a, b) + 1;
                    PathCheck.SetValue(ref name, a, b, val);
                }
                if (val > 100000) name = name.Insert(i, "Copy (1)");
            }
            path += name;
            PathCheck.CheckExistFile(ref path);
        }
    }
}
