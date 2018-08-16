using UnityEngine;

public class TextRender
{
    public static void DrawString(Color[] outBuf, Texture2D font, int x, int y, Color textColor, string s)
    {
        for (int i = 0; i < s.Length; ++i)
        {
            char c = s[i];
            int charIndex = findChar(c);
            if (charIndex < 0)
            {
                continue;
            }
            drawChar(outBuf, font, charIndex, x + 6*i, y, textColor);        
        }    
    }

    public static void SetPixel(Color[] outBuf, int x, int y, Color c)
    {
        if (x < 0 || x>= 64 || y < 0 || y >=64)
        {
            return;
        }
        outBuf[64*y + x] = c;
    }

    static void drawChar(Color[] outBuf, Texture2D font, int c, int x, int y, Color textColor)
    {
        Color32[] fontPixels = font.GetPixels32();

        int fontWidth = font.width;
        int charStart = c * 5;

        for (int i = 0; i < 5; ++i)
        {
            for (int j = 0; j < 7; ++j)
            {
                Color32 fp = fontPixels[charStart + j * fontWidth + i];
                //Debug.Log("fp "+i+" " + j + " " + fp);

                if (fp.a > 127)
                {
                    SetPixel(outBuf, x + i, y + j, textColor);                            
                }
            }
        }        
    }

    static int findChar(char c)
    {
        if (c >= 'A' && c <= 'Z')
        {
            return c - 'A';
        }

        if (c >= '0' && c <= '9')
        {
            return c - '0' + 26;
        }

        if (c == '[')
        {
            return 50;
        }
        if (c == ']')
        {
            return 51;
        }
        if (c == '/')   
        {
            return 52;
        }
        if (c == '\\')
        {
            return 53;
        }
        if (c == '|')
        {
            return 54;
        }
        if (c == '{')
        {
            return 55;
        }
        if (c == '}')
        {
            return 56;
        }
        if (c == '\"')
        {
            return 57;
        }
        if (c == '\'')
        {
            return 58;
        }
        if (c == '.')
        {
            return 59;
        }
        if (c == ',')
        {
            return 60;
        }
        if (c == '<')
        {
            return 61;
        }
        if (c == '>')
        {
            return 62;
        }
        if (c == '?')
        {
            return 63;
        }
        if (c == '!')
        {
            return 36;
        }
        if (c == '@')
        {
            return 37;
        }

        return -1;
    }

}
