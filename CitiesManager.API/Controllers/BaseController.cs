using Microsoft.AspNetCore.Mvc;

namespace CitiesManager.API.Controllers;
// NOTE
// API controllers are simular to MVC Controllers, but surely they are not the same
// Biggest difference is the VIEWS, in APIs we don't have to do anything with views (Components, pages, partials, etc...)
// MVC projects recognize users with SESSIONS, we don't do that in here, user has to introduce itself in each and every request.
// The things that separates API Controllers with MVC Controllers are summed in these two:

// NOTE [ApiController]
// Reasons why we better use [ApiController] attr in api controllers:
// 1. It sends propriate response in case of ModelState error
// 2. It receives the data in JSON format by default and you don't need [FromBody] to get the data as action argument
// 3. It returns the data in JSON by default and you don't have to convert it manually, BUT
//    In case you return ActionResult<T> as result.

// NOTE [ControllerBase]
// ControllerBase is the parent class for Controller, the class that we used to inherit in MVC projects,
// In WebApi projects we don't inherit from Controller and we directly use ControllerBase, 
// Main reason is because Controller has a lot of methods and stuff for views that we don't need in APIs,
// So we can directly use ControllerBase that has every essencial method for controllers except those that envolve views.
[Route("api/v{version:apiVersion}/[controller]")]
[Route("/api/[controller]")]
[ApiController]
public class BaseController : ControllerBase
{
}