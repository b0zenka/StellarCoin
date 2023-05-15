using Microsoft.AspNetCore.Mvc;
using StellarCoin.Models;

namespace StellarCoin.Controllers
{
    public class NetworkController : ControllerBase
    {
        private readonly IBlockChainService blockChain;

        public NetworkController(IBlockChainService blockChain)
        {
            this.blockChain = blockChain;
        }

        [HttpGet]
        [Route("network/status")]
        public IActionResult Status()
        {
            var response = new
            {
                total_nodes = blockChain.GetNodes()
            };
            return Ok(response);
        }
    }
}