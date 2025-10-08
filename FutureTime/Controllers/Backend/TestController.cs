using FutureTime.Service;
using Microsoft.AspNetCore.Mvc;

namespace FutureTime.Controllers.Backend
{
    [Route("notification")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firebaseService"></param>
        public TestController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("test")]
        public async Task<IActionResult> Send(string title, string body, string guestId)
        {
            await _firebaseService.PushNotificationAsync(title, body, null, guestId);
            return Ok();
        }
    }
}
