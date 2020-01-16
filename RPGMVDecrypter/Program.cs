using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RPGMVDecrypter
{
    class Program
    {
        private const string searchPatten = "*.rpgmvp|*.rpgmvo|*.rpgmvm";
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string path = args[0];
                if (Directory.Exists(path))
                {
                    Decrypter d = new Decrypter(path);
                    if (d.readEncryptionkey())
                    {
                        List<string> files = new DirectoryInfo(path).GetCustomFilesPaths(searchPatten,
                            SearchOption.AllDirectories);
                        foreach (string file in files)
                        {
                            if (!d.decryptArrayBuffer(file))
                            {
                                Console.WriteLine("解包失败：{0}", file);
                            }
                        }
                        Console.WriteLine("全部解包完毕");
                    }
                    else
                    {
                        Console.WriteLine("KEY 校验失败");
                    }
                }
            }
            else
            {
                Console.WriteLine(@"无需直接打开exe，请将需要解包的文件夹直接拖放到本exe上，例如：{Project\www} 请将Project文件夹拖放到本exe 进行解包。");
            }
            Console.ReadKey(true);
        }
    }
}
