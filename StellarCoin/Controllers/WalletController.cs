using Microsoft.AspNetCore.Mvc;
using StellarCoin.Models;

namespace StellarCoin.Controllers
{
    public class WalletController : ControllerBase
    {
        private readonly IBlockChainService blockChain;

        public WalletController(IBlockChainService blockChain)
        {
            this.blockChain = blockChain;
        }

        [HttpGet]
        [Route("{address}")]
        public IActionResult Status([FromRoute] string address)
        {
            decimal balance = blockChain.GetWalletBalance(address);

            var response = new
            {
                wallet_address = address,
                balance
            };
            return Ok(response);
        }
    }
}