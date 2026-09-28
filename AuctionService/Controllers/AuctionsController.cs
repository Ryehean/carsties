using System.Globalization;
using AuctionService.Data;
using AuctionService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuctionService.Entities;
using Mapster;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/[controller]")] //http://localhost:7001/api/auctions GET, POST, PUT, DELETE
public class AuctionsController(AuctionDbContext context) : ControllerBase
{
       [HttpGet]
       public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
       {
              var auctions = await context.Auctions
                    
                     .OrderBy(a => a.Item.Make)
                     .ThenBy(a => a.Item.Model)
                     .ProjectToType<AuctionDto>()
                     .ToListAsync();

              return auctions;
       }

       [HttpGet("{id}")]
       public async Task<ActionResult<AuctionDto>> GetAuction(string id)
       {
              var auction = await context.Auctions
                     .ProjectToType<AuctionDto>()
                     .FirstOrDefaultAsync(a => a.Id == id);
              
              if (auction == null)
              {
                     return NotFound();
              }
              return auction;
       }

       [HttpPost]
       public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
       {
              var auction = createAuctionDto.Adapt<Auction>();
              
              //TODO: add current user as seller
              auction.Seller = "TODO: Seller";
              
              context.Auctions.Add(auction);

              await context.SaveChangesAsync();
              
              return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, 
                     auction.Adapt<AuctionDto>());
                     
       }

       [HttpPut("{id}")]
       public async Task<IActionResult> UpdateAuction(string id, UpdateAuctionDto updateAuctionDto)
       {
              var auction = await context.Auctions
                     .Include(x => x.Item)
                     .FirstOrDefaultAsync(x => x.Id == id);

              if (updateAuctionDto.Make == "foo") throw new Exception("bar");
              
              if (auction == null)
              {
                     return NotFound();
              }

              if (auction.CurrentHighBid > 0)
              {
                     return BadRequest("Cannot update an auction that has bids");
              }
              
              //TODO: Check seller is the same as current user
              
              updateAuctionDto.Adapt(auction.Item);

              await context.SaveChangesAsync();
              
              return NoContent();
       }

       [HttpDelete("{id}")]
       public async Task<IActionResult> DeleteAuction(string id)
       {
              var auction = await context.Auctions.FindAsync(id);

              if (auction == null)
              {
                     return NotFound();
              }

              if (auction.CurrentHighBid > 0)
              {
                     return BadRequest("Cannot delete auction that has bids");
              }
              
              //TODO: check the seller is the same as current user
              context.Auctions.Remove(auction);

              await context.SaveChangesAsync();
              return NoContent();
       }
}