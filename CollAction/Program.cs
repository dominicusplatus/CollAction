using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace CollAction
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string certificateFile = "/app/certificate/cert.pfx";

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    if (File.Exists(certificateFile))
                        options.UseHttps(certificateFile);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
