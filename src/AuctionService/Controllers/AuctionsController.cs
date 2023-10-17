using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]

    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
    {
        return _mapper.Map<List<AuctionDTO>>(
            await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync()
            );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        if (auction == null) return NotFound();

        return _mapper.Map<AuctionDTO>(
            auction
            );
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDto auctionDto)
    {
        var auction = _mapper.Map<Auction>(auctionDto);
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Could not save changes to DB");
        return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDTO>(auction));
    }

    [HttpPut("{id}")]

    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
    {
        var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if (!result) return BadRequest("Not Updated");

        return Ok();
    }

}
