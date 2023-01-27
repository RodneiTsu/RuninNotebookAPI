using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RuninNotebookAPI.Models
{
    public class Download
    {

        public MemoryStream DowloadSinghFile(string filename, string uploadPath)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, filename);
            var memory = new MemoryStream();
            if (System.IO.File.Exists(path))
            {
                var net = new System.Net.WebClient();
                var data = net.DownloadData(path);
                var content = new System.IO.MemoryStream(data);
                memory = content;
            }
            memory.Position = 0;
            return memory;
        }


    }
}