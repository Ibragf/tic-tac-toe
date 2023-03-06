using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using tic_tac_toe.Db;
using tic_tac_toe.Models;

namespace tic_tac_toe.Controllers
{
    [Route("api/players")]
    [ApiController]
    [EnableCors("AllowAnyOrigin")]
    public class PlayerController : ControllerBase
    {
        private readonly AppDbContext dbContext;
        public PlayerController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> GetId([Required] string name, [Required] string connectionId)
        {
            Player player = new Player
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ConnectionId = connectionId
            };

            dbContext.Players.Add(player);

            await dbContext.SaveChangesAsync();

            return Ok(player.Id);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConnectionId([Required] string playerId,[Required] string connectionId)
        {
            var player = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == playerId);

            if (player == null) return NotFound();

            player.ConnectionId = connectionId;

            await dbContext.SaveChangesAsync(); 

            return Ok();
        }
    }
}
