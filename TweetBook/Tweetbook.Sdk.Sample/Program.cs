using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Request;

namespace Tweetbook.Sdk.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Console.WriteLine("Hello world!");
            var identityApi = RestService.For<IIdentityAPI>("https://localhost:5001");

            var registerResponse = await identityApi.RegisterAsync(new UserRegistrationRequest
            {
                Email = "hpt2.dut",
                Password = "TrangBK309!"
            });

            var loginResponse = await identityApi.LoginAsync(new UserLoginRequest { 
                Email = "hpt2.dut",
                Password = "TrangBK309!"
            });
        }
    }
}
