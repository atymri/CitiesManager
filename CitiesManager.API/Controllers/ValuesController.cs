using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.API.Controllers;

public class ValuesController : BaseController
{
    [HttpGet("[action]")]
    public string Method() => "Hello, World!";



    [HttpGet("[action]")]
    public string dohteM()
    {
        string response = "Hello, World!";
        return new string(response.Reverse().ToArray());
    }
}